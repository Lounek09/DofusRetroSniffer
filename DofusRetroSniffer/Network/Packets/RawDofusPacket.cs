namespace DofusRetroSniffer.Network.Packets;

/// <summary>
/// Represents a raw Dofus packet.
/// </summary>
/// <param name="IsIncoming">Whether the packet is incoming or outgoing.</param>
/// <param name="DateCaptured">When the packet was captured.</param>
/// <param name="RawData">The raw data of the packet.</param>
public readonly record struct RawDofusPacket(bool IsIncoming, DateTime DateCaptured, string RawData);
