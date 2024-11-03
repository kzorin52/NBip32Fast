using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
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

    public readonly string ToStringPrivate()
    {
        var len = Elements.Length * 5 + 2; // rough approximation

        var sb = len <= 1024 
            ? new ValueStringBuilder(stackalloc char[len]) 
            : new ValueStringBuilder(len);

        sb.Append("m/");

        var last = Elements.Length - 1;

        var elementsSpan = Elements.Span;
        for (var i = 0; i < elementsSpan.Length; i++)
        {
            var element = elementsSpan[i];
            sb.Append((element.Hardened 
                ? element.Number - KeyPathElement.HardenedOffset 
                : element.Number).ToString());
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

        return new KeyPath(elements) { _str = keyPathStr };
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