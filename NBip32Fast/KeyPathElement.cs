using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NBip32Fast;

[StructLayout(LayoutKind.Sequential)]
public readonly struct KeyPathElement(uint number, bool hardened) : IEquatable<KeyPathElement>
{
    internal const uint HardenedOffset = 0x80000000u;

    public static readonly KeyPathElement ZeroHard = new(0u, true);
    public static readonly KeyPathElement ZeroSoft = new(0u, false);

    public readonly uint Number = hardened ? number + HardenedOffset : number;

    public bool Hardened
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Number & HardenedOffset) != 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(Span<byte> output)
    {
        BinaryPrimitives.WriteUInt32BigEndian(output, Number);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyPathElement Hard(uint num)
    {
        return new KeyPathElement(num, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyPathElement Soft(uint num)
    {
        return new KeyPathElement(num, false);
    }

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

    bool IEquatable<KeyPathElement>.Equals(KeyPathElement other)
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