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
| Method        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1   | Allocated  | Alloc Ratio |
|-------------- |----------:|---------:|---------:|------:|--------:|---------:|-------:|-----------:|------------:|
| **NBip39FastKey** |  36.32 us | 0.332 us | 0.310 us |  1.00 |    0.00 |   0.0610 |      - |    2.04 KB |        1.00 |
| NBitcoinKey   | 409.24 us | 3.232 us | 2.865 us | 11.26 |    0.09 |   0.4883 |      - |    9.47 KB |        4.64 |
| NetezosKey    | 556.16 us | 7.795 us | 7.291 us | 15.31 |    0.22 | 166.9922 | 0.9766 | 3083.32 KB |    1,512.13 |

### Ed25519
| Method        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **NBip32FastKey** | 4.272 us | 0.0255 us | 0.0226 us |  1.00 |    0.00 | 0.0839 |   1.59 KB |        1.00 |
| NetezosKey    | 5.404 us | 0.0434 us | 0.0362 us |  1.26 |    0.01 | 0.3204 |   5.99 KB |        3.76 |
| P3HdKey       | 5.939 us | 0.0783 us | 0.1045 us |  1.40 |    0.03 | 0.3433 |   6.33 KB |        3.97 |

### NistP256
| Method        | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1   | Allocated | Alloc Ratio |
|-------------- |-----------:|---------:|---------:|------:|--------:|---------:|-------:|----------:|------------:|
| **NBip39FastKey** |   158.3 us |  1.02 us |  2.37 us |  1.00 |    0.00 |        - |      - |   1.93 KB |        1.00 |
| NetezosKey    | 1,324.4 us | 25.79 us | 40.91 us |  8.37 |    0.29 | 373.0469 | 1.9531 | 6857.9 KB |    3,553.89 |

[Benchmark code](https://github.com/kzorin52/NBip32Fast/blob/master/NBip32Fast.Benchmark/)


## TODOs
- [ ] Ed25519 soft derivation scheme (used in `Cardano`)
- [ ] `KeyPathTree` for efficient computing (for multiple keypath merging and index depth)
- [ ] `HDKey` refactoring with public key lazy addition
- [ ] More testing