using DofusRetroSniffer.Utils;

using PacketDotNet;

using SharpPcap;
using SharpPcap.LibPcap;

using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DofusRetroSniffer;

/// <summary>
/// Represents a network packet sniffer for Dofus Retro.
/// </summary>
public sealed class Sniffer
{
    private readonly LibPcapLiveDevice _device;
    private readonly IPAddress _localIP;

    private readonly List<byte> _receivebuffer = [];
    private readonly List<byte> _sendbuffer = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Sniffer"/> class.
    /// </summary>
    /// <param name="config">The configuration for the sniffer.</param>
    /// <param name="device">The network device to capture packets from.</param>
    public Sniffer(SnifferConfig config, LibPcapLiveDevice device)
    {
        _device = device;
        _localIP = IPAddress.Parse(config.LocalIp);

        _device.OnPacketArrival += Device_OnPacketArrival;
        _device.Open(new DeviceConfiguration()
        {
            LinkLayerType = LinkLayers.Ethernet
        });
        _device.Filter = $"ip host {string.Join(" or ", config.Servers)} and host {config.LocalIp} and tcp and port {config.GamePort}";
    }

    /// <summary>
    /// Starts listening for packets on the configured network device.
    /// </summary>
    public void Listen()
    {
        _device.StartCapture();
    }

    private void Device_OnPacketArrival(object _, PacketCapture e)
    {
        var rawCapture = e.GetPacket();
        var packet = rawCapture.GetPacket();
        var ipPacket = (IPv4Packet)packet.PayloadPacket;
        var tcpPacket = (TcpPacket)ipPacket.PayloadPacket;

        var rawData = tcpPacket.PayloadData;
        var isIncoming = ipPacket.DestinationAddress.Equals(_localIP);

        if (rawData.Length > 0)
        {
            var buffer = isIncoming ? _receivebuffer : _sendbuffer;
            buffer.AddRange(rawData);

            // Each sent/received packet from the flash XMLSocket class is terminated by a zero (0) byte
            var indexOfSeparator = buffer.IndexOf(0);

            while (indexOfSeparator != -1)
            {
                var rawDofusDataSpan = CollectionsMarshal.AsSpan(buffer);

                // Each sent packet from the game client is also terminated by a 0x0A (10) byte
                rawDofusDataSpan = rawDofusDataSpan[..(indexOfSeparator - (isIncoming ? 0 : 1))];

                // Since the 1.39.5 update, each sent packet is prefixed with telemetry data enclosed in 0xC3 0xB9 (195 185) bytes
                if (!isIncoming && rawDofusDataSpan.Length > 1 && rawDofusDataSpan[0] == 195 && rawDofusDataSpan[1] == 185)
                {
                    rawDofusDataSpan = rawDofusDataSpan[2..];

                    var indexOfEndTelemetry = rawDofusDataSpan.IndexOf((byte)195);

                    while (indexOfEndTelemetry != -1 && indexOfEndTelemetry < rawDofusDataSpan.Length - 1)
                    {
                        if (rawDofusDataSpan[indexOfEndTelemetry + 1] == 185)
                        {
                            rawDofusDataSpan = rawDofusDataSpan[(indexOfEndTelemetry + 2)..];
                            break;
                        }

                        rawDofusDataSpan = rawDofusDataSpan[(indexOfEndTelemetry + 1)..];
                        indexOfEndTelemetry = rawDofusDataSpan.IndexOf((byte)195);
                    }
                }

                var dofusData = Encoding.UTF8.GetString(rawDofusDataSpan);
                PacketLogger.Write(dofusData, isIncoming, e.Header.Timeval.Date);

                buffer.RemoveRange(0, indexOfSeparator + 1);
                indexOfSeparator = buffer.IndexOf(0);
            }
        }
    }
}
