using System.Text;

namespace NBip32Fast;

public struct KeyPath(KeyPathElement[] elements)
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
        var len = Elements.Length * 5 + 2; // rough approximation

        var sb = len <= 1024 
            ? new ValueStringBuilder(stackalloc char[len]) 
            : new ValueStringBuilder(len);

        sb.Append("m/");

        var last = Elements.Length - 1;

        for (var i = 0; i < Elements.Length; i++)
        {
            var element = Elements[i];
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
        var strSpan = keyPathStr.AsSpan();

        Span<Range> keys = stackalloc Range[strSpan.Count('/') + 1];
        if (keys.Length == 0) return Empty;

        var written = strSpan.Split(keys, '/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (written <= 0) return Empty;

        keys = keys[..written];
        if (strSpan[0] == 'm') keys = keys[1..];

        var elements = new KeyPathElement[keys.Length];
        for (var i = 0; i < keys.Length; i++)
        {
            var key = strSpan[keys[i]];
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
               && Elements.AsSpan().SequenceEqual(other.Elements);
    }

    public readonly override int GetHashCode()
    {
        if (Elements.Length == 0) return -1;

        var hs = new HashCode();
        for (var i = 0; i < Elements.Length; i++) hs.Add(Elements[i].Number);

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