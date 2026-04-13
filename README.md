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
> AMD Ryzen Threadripper 7980X 64-Cores 2.18GHz, 1 CPU, 128 logical and 64 physical cores (lower values is better)

### SecP256K1
| Method            |      Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------|----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **NBip32FastKey** |  36.08 us | 0.042 us | 0.037 us |  1.00 |    0.00 |     104 B |        1.00 |                                                                                                                                                                                                                    
| NBitcoinKey       | 884.07 us | 3.557 us | 3.327 us | 24.50 |    0.09 |    9296 B |       89.38 |
| NetezosKey        | 931.24 us | 5.300 us | 4.958 us | 25.81 |    0.14 | 3281497 B |   31,552.86 |

### Ed25519
| Method            |     Mean |     Error |    StdDev | Ratio | Allocated | Alloc Ratio |
|-------------------|---------:|----------:|----------:|------:|----------:|------------:|
| **NBip32FastKey** | 7.186 us | 0.0119 us | 0.0106 us |  1.00 |     104 B |        1.00 |                                                                                                                                                                                                                             
| NetezosKey        | 8.738 us | 0.0243 us | 0.0216 us |  1.22 |    5448 B |       52.38 |
| P3HdKey           | 9.460 us | 0.0338 us | 0.0300 us |  1.32 |    6112 B |       58.77 |

### NistP256
| Method            |       Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------|-----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **NBip32FastKey** |   167.2 us |  0.53 us |  0.49 us |  1.00 |    0.00 |     608 B |        1.00 |
| NetezosKey        | 1,483.8 us | 11.86 us | 10.51 us |  8.87 |    0.07 | 7029510 B |   11,561.69 |

### Parse + .ToString() KeyPath
| Method              |      Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------------|----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **NBip32FastParse** |  41.12 ns | 0.071 ns | 0.060 ns |  1.00 |    0.00 |      56 B |        1.00 |                                                                                                                                                                                                                  
| NBitcoinParse       | 292.12 ns | 1.571 ns | 1.469 ns |  7.10 |    0.04 |    1104 B |       19.71 |
| NetezosParse        | 317.28 ns | 1.028 ns | 0.962 ns |  7.72 |    0.03 |     992 B |       17.71 |

[Benchmark code](https://github.com/kzorin52/NBip32Fast/blob/master/NBip32Fast.Benchmark/)


## TODOs
- [ ] Ed25519 soft derivation scheme (used in `Cardano`)
- [ ] `KeyPathTree` for efficient computing (for multiple keypath merging and index depth)
- [ ] `HDKey` refactoring with public key lazy addition
- [ ] More testing