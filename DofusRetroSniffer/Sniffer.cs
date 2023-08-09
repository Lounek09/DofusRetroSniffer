using DofusRetroSniffer.Utils;

using PacketDotNet;

using SharpPcap;
using SharpPcap.LibPcap;

using System.Text;

namespace DofusRetroSniffer
{
    public class Sniffer
    {
        private readonly Config _config;

        private readonly LibPcapLiveDevice _device;
        private readonly List<byte> _receivebuffer = new();
        private readonly List<byte> _sendbuffer = new();

        public Sniffer(Config config)
        {
            _config = config;

            _device = FindDevice(_config.LocalIp) ?? throw new NullReferenceException($"Unable to find the device '{_config.LocalIp}' to listen to");
            _device.OnPacketArrival += Device_OnPacketArrival;
            _device.Open(new DeviceConfiguration()
            {
                LinkLayerType = LinkLayers.Ethernet
            });
            _device.Filter = $"ip host {string.Join(" or ", _config.Servers)} and host {_config.LocalIp} and tcp and port {_config.GamePort}";
            _device.StartCapture();
        }

        private static LibPcapLiveDevice? FindDevice(string ip)
        {
            foreach (LibPcapLiveDevice device in LibPcapLiveDeviceList.Instance)
            {
                foreach (PcapAddress adress in device.Addresses)
                {
                    if (adress.Addr.ToString().Equals(ip))
                        return device;
                }
            }

            return null;
        }

        private void Device_OnPacketArrival(object _, PacketCapture e)
        {
            RawCapture rawPacket = e.GetPacket();
            Packet packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
            TcpPacket tcpPacket = packet.Extract<TcpPacket>();
            IPv4Packet ipPacket = (IPv4Packet)tcpPacket.ParentPacket;

            bool isIncoming = ipPacket.DestinationAddress.ToString().Equals(_config.LocalIp);

            //Translate PayloadData
            byte[] rawData = tcpPacket.PayloadData;
            if (rawData.Length > 0)
            {
                List<byte> buffer = isIncoming ? _receivebuffer : _sendbuffer;
                buffer.AddRange(rawData);

                byte separator = (byte)(isIncoming ? 0 : 10);
                int posSeparator = buffer.IndexOf(separator);
                while (posSeparator != -1)
                {
                    List<byte> rawDofusData = buffer.GetRange(0, posSeparator);
                    string dofusData = Encoding.UTF8.GetString(rawDofusData.ToArray());

                    if (!isIncoming && dofusData.StartsWith('ù')) //Remove the post processing part from 1.39.5 update
                    {
                        int posEndPostProcessing = dofusData.IndexOf('ù', 1);

                        if (posEndPostProcessing != -1)
                            dofusData = dofusData[(posEndPostProcessing + 1)..];
                    }

                    Logger.Packet(dofusData, isIncoming, e.Header.Timeval.Date);

                    buffer.RemoveRange(0, posSeparator + (isIncoming ? 1 : 2));
                    posSeparator = buffer.IndexOf(separator);
                }
            }
        }
    }
}
