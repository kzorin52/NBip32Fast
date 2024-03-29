[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.svg)](https://www.nuget.org/packages/NBip32Fast)
[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.Secp256K1.svg)](https://www.nuget.org/packages/NBip32Fast.Secp256K1)
[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.Ed25519.svg)](https://www.nuget.org/packages/NBip32Fast.Ed25519)
[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.NistP256.svg)](https://www.nuget.org/packages/NBip32Fast.NistP256)

# NBip32Fast
*High perfomance BIP-32 HD key derivation library for .NET 8*

## Usage
### Basic
```cs
var secp256k1Key = Secp256K1.Secp256K1HdKey.Instance.DerivePath("m/44'/0'/0'/0/0", seed).Key;
var nistP256Key = NistP256.NistP256HdKey.Instance.DerivePath("m/44'/0'/0'/0/0", seed).Key;
var ed25519Key = Ed25519.Ed25519HdKey.Instance.DerivePath("m/44'/0'/0'/0'/0'", seed).Key;
```

### Optimised
```cs
var master = Ed25519.Ed25519HdKey.Instance.DerivePath("m/44'/888'/0'/0'", seed);
var accounts = new List<byte[]>();

for (var i = 0u; i < 5u; i++)
{
    accounts.Add(Ed25519.Ed25519HdKey.Instance.GetPublic(Ed25519.Ed25519HdKey.Instance.Derive(master, new KeyPathElement(i, true)).Key));
}
```

## Benchmarks
> Intel Core i9-14900K, 1 CPU, 32 logical and 24 physical cores

### SecP256K1
| Method        | Mean      | Error    | StdDev   | Gen0     | Gen1   | Allocated  |
|-------------- |----------:|---------:|---------:|---------:|-------:|-----------:|
| **NBip39FastKey** |  37.91 us | 0.112 us | 0.105 us |   0.0610 |      - |    2.04 KB |
| NBitcoinKey   | 448.31 us | 1.104 us | 1.033 us |   0.4883 |      - |    9.38 KB |
| NetezosKey    | 619.23 us | 4.850 us | 4.537 us | 168.9453 | 0.9766 | 3112.87 KB |

### Ed25519
| Method        | Mean     | Error     | StdDev    | Gen0   | Allocated |
|-------------- |---------:|----------:|----------:|-------:|----------:|
| **NBip32FastKey** | 4.572 us | 0.0104 us | 0.0097 us | 0.0839 |   1.59 KB |
| NetezosKey    | 5.796 us | 0.0257 us | 0.0241 us | 0.3204 |   5.95 KB |
| P3HdKey       | 6.173 us | 0.0103 us | 0.0126 us | 0.3357 |   6.28 KB |

### NistP256
| Method        | Mean       | Error    | StdDev  | Gen0     | Gen1   | Allocated  |
|-------------- |-----------:|---------:|--------:|---------:|-------:|-----------:|
| **NBip39FastKey** |   161.6 us |  0.19 us | 0.17 us |        - |      - |    1.93 KB |
| NetezosKey    | 1,429.4 us | 10.00 us | 8.86 us | 373.0469 | 1.9531 | 6888.66 KB |

[Benchmark code](https://github.com/kzorin52/NBip32Fast/blob/master/NBip32Fast.Benchmark/)


## TODOs
- [ ] Ed25519 soft derivation scheme (used in `Cardano`)
- [ ] `KeyPathTree` for efficient computing (for multiple keypath merging and index depth)
- [ ] `HDKey` refactoring with public key lazy addition
- [ ] `Secp256K1` public key without `Span<byte>` to `byte[]` conversion
    - [ ] All providers `ReadOnlySpan<byte>` public keys
- [ ] More testing