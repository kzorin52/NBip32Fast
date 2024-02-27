namespace NBip32Fast;

public readonly ref struct HdKey(ReadOnlySpan<byte> privateKey, ReadOnlySpan<byte> cc)
{
    public readonly ReadOnlySpan<byte> PrivateKey = privateKey;
    public readonly ReadOnlySpan<byte> ChainCode = cc;
}