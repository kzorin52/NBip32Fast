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
| Method        | Mean      | Error    | StdDev   |
|-------------- |----------:|---------:|---------:|
| **NBip39FastKey** |  38.10 us | 0.329 us | 0.308 us |
| NBitcoinKey   | 454.49 us | 0.789 us | 0.700 us |
| NetezosKey    | 647.82 us | 4.799 us | 4.254 us |

### Ed25519
| Method        | Mean     | Error     | StdDev    |
|-------------- |---------:|----------:|----------:|
| **NBip32FastKey** | 4.561 us | 0.0242 us | 0.0227 us |
| NetezosKey    | 5.962 us | 0.0300 us | 0.0281 us |
| P3HdKey       | 6.264 us | 0.1115 us | 0.1489 us |

### NistP256
| Method        | Mean       | Error   | StdDev  |
|-------------- |-----------:|--------:|--------:|
| **NBip39FastKey** |   163.8 us | 0.26 us | 0.32 us |
| NetezosKey    | 1,447.5 us | 5.16 us | 4.57 us |

[Benchmark code](https://github.com/kzorin52/NBip32Fast/blob/master/NBip32Fast.Benchmark/Program.cs)
