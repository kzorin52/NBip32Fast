using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NBip32Fast;

public struct KeyPath(KeyPathElement[] elements) : IEquatable<KeyPath>
{
    public static readonly KeyPath Empty = new([]);

    public readonly KeyPathElement[] Elements = elements;
    private string? _str = null;

    #region To string

    public override string ToString()
    {
        return _str ??= ToStringPrivate();
    }

    private readonly string ToStringPrivate()
    {
        if (Elements.Length == 0) return "m";

        var maxLen = 2 + Elements.Length * 12;

        var sb = maxLen <= 1024
            ? new ValueStringBuilder(stackalloc char[maxLen])
            : new ValueStringBuilder(maxLen);

        sb.Append("m/");

        for (var i = 0; i < Elements.Length; i++)
        {
            if (i != 0) sb.Append('/');

            var num = Elements[i].Number;

            sb.AppendSpanFormattable(num & 0x7FFFFFFFu);

            if (num >= KeyPathElement.HardenedOffset)
                sb.Append('\'');
        }

        return sb.ToString();
    }

    public static implicit operator string(KeyPath path)
    {
        return path.ToString();
    }

    #endregion

    #region From string

    public static KeyPath Parse(string keyPathStr)
    {
        var span = keyPathStr.AsSpan();
        
        switch (span)
        {
            case ['m', '/', ..]:
                span = span[2..];
                break;
            case ['m']:
                return new KeyPath([]) { _str = keyPathStr };
        }

        if (span.IsEmpty)
            return new KeyPath([]) { _str = keyPathStr };

        var elements = new KeyPathElement[span.Count('/') + 1];
        var i = 0;
        
        foreach (var range in span.Split('/'))
        {
            var segment = span[range];
            if (segment.IsEmpty) continue;

            var hardened = segment[^1] is '\'';
            elements[i++] = new KeyPathElement(
                ParseUInt32(hardened ? segment[..^1] : segment),
                hardened
            );
        }

        // e.g. "m/44'//0'"
        if (i != elements.Length)
            elements = elements.AsSpan(0, i).ToArray();

        return new KeyPath(elements) { _str = keyPathStr };
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ParseUInt32(ReadOnlySpan<char> span)
    {
        var result = 0u;
        foreach (var c in span)
            result = result * 10 + (uint)(c - '0');
        return result;
    }

    public static implicit operator KeyPath(string str)
    {
        return Parse(str);
    }

    #endregion

    #region Equality

    [OverloadResolutionPriority(1)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(ref readonly KeyPath other)
    {
        if (_str != null && other._str != null) return _str == other._str;
        
        return MemoryMarshal.AsBytes(Elements.AsSpan()).SequenceEqual(MemoryMarshal.AsBytes(other.Elements.AsSpan()));
    }

    bool IEquatable<KeyPath>.Equals(KeyPath other)
    {
        return Equals(ref other);
    }
    
    public readonly override bool Equals(object? obj) => obj is KeyPath other && Equals(in other);

    public readonly override int GetHashCode()
    {
        var hc = new HashCode();
        hc.AddBytes(MemoryMarshal.AsBytes(Elements.AsSpan()));
        return hc.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in KeyPath left, in KeyPath right) => left.Equals(in right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in KeyPath left, in KeyPath right) => !left.Equals(in right);

    #endregion
}