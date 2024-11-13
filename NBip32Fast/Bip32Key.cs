using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NBip32Fast;

[StructLayout(LayoutKind.Sequential, Size = 64)]
public ref struct Bip32Key
{
    public unsafe Span<byte> Span => new(Unsafe.AsPointer(ref this), 64);

    public unsafe ReadOnlySpan<byte> Key => new(Unsafe.AsPointer(ref this), 32);
    public unsafe ReadOnlySpan<byte> ChainCode => new((byte*)Unsafe.AsPointer(ref this) + 32, 32);
}