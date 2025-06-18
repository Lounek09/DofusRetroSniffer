using DofusRetroSniffer.Utils;

using PacketDotNet;

using Serilog;

using SharpPcap;
using SharpPcap.LibPcap;

using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DofusRetroSniffer;

/// <summary>
/// Represents a network packet sniffer for Dofus Retro.
/// </summary>
public interface ISniffer
{
    /// <summary>
    /// Starts capturing packets on the configured network device.
    /// </summary>
    void StartCapture();

    /// <summary>
    /// Stops capturing packets on the configured network device.
    /// </summary>
    void StopCapture();
}

public sealed class Sniffer : ISniffer
{
    private readonly ILogger _logger;
    private readonly IPacketLogger _packetLogger;

    private readonly IPAddress _localIP;
    private readonly LibPcapLiveDevice _device;
    private readonly List<byte> _receivebuffer;
    private readonly List<byte> _sendbuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sniffer"/> class.
    /// </summary>
    /// <param name="config">The sniffer configuration.</param>
    /// <param name="packetLogger"> The logger to use for logging captured packets.</param>
    public Sniffer(SnifferConfig config, IPacketLogger packetLogger)
    {
        _logger = Log.ForContext<Sniffer>();
        _packetLogger = packetLogger;

        _localIP = IPAddress.Parse(config.LocalIp);
        _device = FindDevice();
        _device.Open(new DeviceConfiguration()
        {
            LinkLayerType = LinkLayers.Ethernet
        });
        _device.Filter = $"ip host {string.Join(" or ", config.Servers)} and host {config.LocalIp} and tcp and port {config.GamePort}";
        _receivebuffer = [];
        _sendbuffer = [];
    }

    public void StartCapture()
    {
        _device.OnPacketArrival += Device_OnPacketArrival;

        _device.StartCapture();

        _logger.Debug("Started capturing packets");
    }

    public void StopCapture()
    {
        _device.OnPacketArrival -= Device_OnPacketArrival;

        _device.StopCapture();

        _logger.Debug("Stopped capturing packets");
    }

    /// <summary>
    /// Finds the network device that matches the specified local IP address.
    /// </summary>
    /// <returns>The <see cref="LibPcapLiveDevice"/> instance corresponding to the specified IP address.</returns>
    /// <exception cref="NullReferenceException">Thrown if no device is found that matches the specified IP address.</exception>
    private LibPcapLiveDevice FindDevice()
    {
        foreach (var device in LibPcapLiveDeviceList.Instance)
        {
            foreach (var address in device.Addresses)
            {
                if (_localIP.Equals(address.Addr?.ipAddress))
                {
                    return device;
                }
            }
        }

        throw new InvalidOperationException($"Unable to find the device with IP '{_localIP}' to listen to");
    }

    private void Device_OnPacketArrival(object _, PacketCapture e)
    {
        var rawCapture = e.GetPacket();
        var packet = rawCapture.GetPacket();
        var ipPacket = (IPv4Packet)packet.PayloadPacket;
        var tcpPacket = (TcpPacket)ipPacket.PayloadPacket;

        var rawData = tcpPacket.PayloadData;
        var isIncoming = ipPacket.DestinationAddress.Equals(_localIP);

        _logger.Debug(
            "Packet: {Direction} | Source: {Source} | Destination: {Destination} | Length: {Length}",
            isIncoming ? "INCOMING" : "OUTGOING",
            ipPacket.SourceAddress,
            ipPacket.DestinationAddress,
            rawData.Length);

        if (rawData.Length > 0)
        {
            var buffer = isIncoming ? _receivebuffer : _sendbuffer;
            buffer.AddRange(rawData);

            // Each sent/received packet from the flash XMLSocket class is terminated by a zero (0) byte
            var indexOfSeparator = buffer.IndexOf(0);

            while (indexOfSeparator != -1)
            {
                if (indexOfSeparator == 0)
                {
                    buffer.RemoveAt(0);
                    indexOfSeparator = buffer.IndexOf(0);

                    continue;
                }

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

                var rawDofusData = Encoding.UTF8.GetString(rawDofusDataSpan);
                var dateReceived = e.Header.Timeval.Date;

                _packetLogger.Write(isIncoming, dateReceived, rawDofusData);

                buffer.RemoveRange(0, indexOfSeparator + 1);
                indexOfSeparator = buffer.IndexOf(0);
            }
        }
    }
}
