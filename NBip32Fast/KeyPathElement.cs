using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace NBip32Fast;

public readonly struct KeyPathElement(uint number, bool hardened)
{
    internal const uint HardenedOffset = 0x80000000u;

    public static readonly KeyPathElement ZeroHard = new(0u, true);
    public static readonly KeyPathElement ZeroSoft = new(0u, false);

    public readonly uint Number = hardened ? number + HardenedOffset : number;
    public readonly bool Hardened = hardened;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(Span<byte> output)
    {
        BinaryPrimitives.WriteUInt32BigEndian(output, Number);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyPathElement Hard(uint num) => new(num, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyPathElement Soft(uint num) => new(num, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator uint(KeyPathElement el)
    {
        return el.Number;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<byte>(KeyPathElement el)
    {
        var alloc = new byte[4];
        el.Serialize(alloc);

        return alloc;
    }

    #region Equality

    public override bool Equals(object? obj)
    {
        return obj is KeyPathElement other && Equals(other);
    }

    public bool Equals(KeyPathElement other)
    {
        return Number == other.Number;
    }

    public override int GetHashCode()
    {
        return (int)Number;
    }

    public static bool operator ==(KeyPathElement left, KeyPathElement right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(KeyPathElement left, KeyPathElement right)
    {
        return !(left == right);
    }

    #endregion
}