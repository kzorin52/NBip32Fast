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
| **NBip32FastKey** |  32.49 us | 0.088 us | 0.083 us |  1.00 |    0.00 |     120 B |        1.00 |
| NBitcoinKey   | 447.77 us | 1.211 us | 1.074 us | 13.78 |    0.05 |    9664 B |       80.53 |
| NetezosKey    | 635.40 us | 1.744 us | 1.546 us | 19.55 |    0.07 | 3185946 B |   26,549.55 |

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
| Method          | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| **NBip32FastParse** |  73.83 ns | 0.227 ns | 0.212 ns |  1.00 |    0.00 |      80 B |        1.00 |
| NBitcoinParse   | 243.23 ns | 1.687 ns | 1.317 ns |  3.29 |    0.02 |    1168 B |       14.60 |
| NetezosParse    | 278.73 ns | 0.836 ns | 0.782 ns |  3.78 |    0.01 |    1184 B |       14.80 |

[Benchmark code](https://github.com/kzorin52/NBip32Fast/blob/master/NBip32Fast.Benchmark/)


## TODOs
- [ ] Ed25519 soft derivation scheme (used in `Cardano`)
- [ ] `KeyPathTree` for efficient computing (for multiple keypath merging and index depth)
- [ ] `HDKey` refactoring with public key lazy addition
- [ ] More testing