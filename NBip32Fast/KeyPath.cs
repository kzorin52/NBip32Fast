using System.Text;

namespace NBip32Fast;

public struct KeyPath(KeyPathElement[] elements)
{
    public static readonly KeyPath Empty = new([]);

    public readonly ReadOnlyMemory<KeyPathElement> Elements = new(elements);
    private string? _str = null;

    #region To string

    public override string ToString()
    {
        return _str ??= ToStringPrivate();
    }

    private readonly string ToStringPrivate()
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

    public static KeyPath Parse(string keyPathStr)
    {
        var keys = keyPathStr.Split('/').AsSpan();
        if (keys.Length == 0) return Empty;
        if (keyPathStr[0] == 'm') keys = keys[1..];

        var elements = new KeyPathElement[keys.Length];
        for (var i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            var hardened = key[^1] == '\'';

            elements[i] = new KeyPathElement(uint.Parse(hardened ? key[..^1] : key), hardened);
        }

        return new KeyPath(elements) { _str = keyPathStr };
    }

    public static implicit operator KeyPath(string str)
    {
        return Parse(str);
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