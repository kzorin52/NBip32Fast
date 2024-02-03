using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NBip32Fast;

public struct KeyPath(KeyPathElement[] elements)
{
    public readonly ReadOnlyMemory<KeyPathElement> Elements = new(elements);
    private string? _str = null;

    #region To string

    public override string ToString()
    {
        return _str ??= ToStringPrivate();
    }

    private readonly string ToStringPrivate()
    {
        var sb = new StringBuilder("m/");
        var last = Elements.Length - 1;
        for (var i = 0; i < Elements.Length; i++)
        {
            var element = Elements.Span[i];
            sb.Append(element.Hardened ? element.Number - KeyPathElement.HardenedOffset : element.Number);
            if (element.Hardened)
                sb.Append('\'');
            if (i != last) sb.Append('/');
        }

        return sb.ToString();
    }

    public static implicit operator string(KeyPath path)
    {
        return path.ToString();
    }

    #endregion

    #region From string

    public static KeyPath? Parse(in string keyPathStr)
    {
        var keys = keyPathStr.Split('/').AsSpan();
        if (keys.Length == 0) return null;
        if (keyPathStr[0] == 'm') keys = keys[1..];

        var elements = new KeyPathElement[keys.Length];
        for (var i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            var hardened = key.Last() == '\'';

            elements[i] = new KeyPathElement(uint.Parse(hardened ? key[..^1] : key), hardened);
        }

        return new KeyPath(elements);
    }

    public static implicit operator KeyPath(string str)
    {
        return Parse(str)!.Value;
    }

    #endregion

    #region Equality

    public readonly override bool Equals(object? obj)
    {
        return obj is KeyPath other
               && Elements.Span.SequenceEqual(other.Elements.Span);
    }

    public readonly override int GetHashCode()
    {
        if (Elements.Span.IsEmpty) return 0;

        var hs = new HashCode();
        for (var i = 0; i < Elements.Span.Length; i++) hs.Add(Elements.Span[i].Number);

        return hs.ToHashCode();
    }

    public static bool operator ==(KeyPath left, KeyPath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(KeyPath left, KeyPath right)
    {
        return !(left == right);
    }

    #endregion
}

public readonly struct KeyPathElement
{
    internal const uint HardenedOffset = 0x80000000u;

    public static readonly KeyPathElement ZeroHard = new(0u, true);
    public static readonly KeyPathElement ZeroSoft = new(0u, false);

    public readonly uint Number;
    public readonly bool Hardened;
    public readonly ReadOnlyMemory<byte> Serialized;

    public KeyPathElement(uint number, bool hardened)
    {
        Number = hardened ? number + HardenedOffset : number;
        Hardened = hardened;

        if (number < 100ul)
        {
            Serialized = hardened 
                ? SerCache.HardCache.Span[(int)number] 
                : SerCache.SoftCache.Span[(int)number];
            return;
        }

        Serialized = SerializeUInt32(Number);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> SerializeUInt32(in uint index)
    {
        Memory<byte> ser = new byte[4];
        MemoryMarshal.Write(ser.Span, in index);

        if (BitConverter.IsLittleEndian)
            ser.Span.Reverse(); // change endianness

        return ser;
    }
}

public static class SerCache
{
    public static readonly ReadOnlyMemory<ReadOnlyMemory<byte>> SoftCache = FillCache(false);
    public static readonly ReadOnlyMemory<ReadOnlyMemory<byte>> HardCache = FillCache(true);

    private static ReadOnlyMemory<ReadOnlyMemory<byte>> FillCache(bool hard)
    {
        var result = new ReadOnlyMemory<byte>[100];
        for (var i = 0u; i < 100u; i++)
        {
            result[i] = KeyPathElement.SerializeUInt32(hard ? i + KeyPathElement.HardenedOffset : i);
        }

        return new ReadOnlyMemory<ReadOnlyMemory<byte>>(result);
    }
}
// WARNING! CURSED CODE!