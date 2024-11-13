[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.svg)](https://www.nuget.org/packages/NBip32Fast)
[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.Secp256K1.svg)](https://www.nuget.org/packages/NBip32Fast.Secp256K1)
[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.Ed25519.svg)](https://www.nuget.org/packages/NBip32Fast.Ed25519)
[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.NistP256.svg)](https://www.nuget.org/packages/NBip32Fast.NistP256)

# NBip32Fast
*High perfomance and low allocation BIP-32 HD key derivation library for .NET 9+*

## Usage
### Basic
```cs
var der = new Bip32Key();

Secp256K1HdKey.Instance.DerivePath("m/44'/0'/0'/0/0", seed, ref der);
NistP256HdKey.Instance.DerivePath("m/44'/0'/0'/0/0", seed, ref der);
Ed25519HdKey.Instance.DerivePath("m/44'/0'/0'/0'/0'", seed, ref der);
```

### Extended (tree-like)
```cs
var master = new Bip32Key();
Ed25519HdKey.Instance.DerivePath("m/44'/888'/0'/0'", seed, ref master);

var accounts = new List<byte[]>();

var derResult = new Bip32Key();
for (var i = 0u; i < 5u; i++)
{
    Ed25519HdKey.Instance.Derive(ref master, KeyPathElement.Hard(i), ref derResult);
    accounts.Add(Ed25519HdKey.Instance.GetPublic(derResult.Key));
}
```

## Benchmarks
> Intel Core i9-14900K, 1 CPU, 32 logical and 24 physical cores (lower values is better)

### SecP256K1
| Method        | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **NBip32FastKey** |  33.93 us | 0.239 us | 0.224 us |  1.00 |    0.01 |     608 B |        1.00 |
| NBitcoinKey   | 512.24 us | 1.668 us | 1.393 us | 15.10 |    0.10 |    9665 B |       15.90 |
| NetezosKey    | 655.89 us | 2.671 us | 2.367 us | 19.33 |    0.14 | 3200386 B |    5,263.79 |

### Ed25519
| Method        | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------- |---------:|----------:|----------:|------:|----------:|------------:|
| **NBip32FastKey** | 4.527 us | 0.0217 us | 0.0193 us |  1.00 |     672 B |        1.00 |
| NetezosKey    | 6.041 us | 0.0258 us | 0.0242 us |  1.33 |    6136 B |        9.13 |
| P3HdKey       | 6.415 us | 0.0626 us | 0.0555 us |  1.42 |    6480 B |        9.64 |

### NistP256
| Method        | Mean       | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------- |-----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **NBip32FastKey** |   167.2 us |  0.53 us |  0.49 us |  1.00 |    0.00 |     608 B |        1.00 |
| NetezosKey    | 1,483.8 us | 11.86 us | 10.51 us |  8.87 |    0.07 | 7029510 B |   11,561.69 |

### Parse KeyPath
| Method                 | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **NBip32FastParse**    | 103.2 ns | 0.84 ns | 0.74 ns |  1.00 |    0.01 | 0.0272 |     512 B |        1.00 |
| NBitcoinParse          | 247.6 ns | 3.63 ns | 3.22 ns |  2.40 |    0.03 | 0.0620 |    1168 B |        2.28 |
| NetezosParse           | 275.2 ns | 1.72 ns | 1.52 ns |  2.67 |    0.02 | 0.0625 |    1184 B |        2.31 |

[Benchmark code](https://github.com/kzorin52/NBip32Fast/blob/master/NBip32Fast.Benchmark/)


## TODOs
- [ ] Ed25519 soft derivation scheme (used in `Cardano`)
- [ ] `KeyPathTree` for efficient computing (for multiple keypath merging and index depth)
- [ ] `HDKey` refactoring with public key lazy addition
- [ ] More testing