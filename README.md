[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.svg)](https://www.nuget.org/packages/NBip32Fast)

# NBip32Fast
*High perfomance BIP-32 HD key derivation library for .NET 8*

## Usage
### Basic
```cs
var secp256k1Key = NBip32Fast.Derivation.Secp256K1.DerivePath("m/44'/0'/0'/0/0", seed).Key;
var ed25519Key = NBip32Fast.Derivation.Ed25519.DerivePath("m/44'/0'/0'/0'/0'", seed).Key;
var nistP256Key = NBip32Fast.Derivation.NistP256.DerivePath("m/44'/0'/0'/0/0", seed).Key;
```

### Optimised
```cs
var master = Derivation.Ed25519.DerivePath("m/44'/888'/0'/0", seed);
var accounts = new List<byte[]>();

for (var i = 0u; i < 5u; i++)
{
    accounts.Add(Derivation.Ed25519.GetPublic(Derivation.Ed25519.Derive(master, new KeyPathElement(i, true)).Key));
}
```

## Benchmarks
### SecP256K1
| Method        | Mean      | Error    | StdDev   |
|:--------------|----------:|---------:|---------:|
| NBitcoinKey   | 681.74 us | 5.098 us | 4.519 us |
| **NBip39FastKey** | **56.36 us** | 0.409 us | 0.382 us |
| NetezosKey    | 957.96 us | 5.120 us | 3.998 us |

### Ed25519
| Method        | Mean     | Error     | StdDev    |
|:--------------|---------:|----------:|----------:|
| P3HdKey       | 9.413 us | 0.0886 us | 0.0829 us |
| **NBip32FastKey** | **6.944 us** | 0.0498 us | 0.0442 us |
| NetezosKey    | 8.934 us | 0.1022 us | 0.0956 us |

### NistP256
| Method        | Mean       | Error    | StdDev   |
|:--------------|-----------:|---------:|---------:|
| **NBip39FastKey** | **239.8 us** |  1.09 us |  0.96 us |
| NetezosKey    | 2,183.8 us | 29.81 us | 27.88 us |

[Benchmark code](https://github.com/kzorin52/NBip32Fast/blob/master/NBip32Fast.Benchmark/Program.cs)
