[![NuGet](https://img.shields.io/nuget/v/NBip32Fast.svg)](https://www.nuget.org/packages/NBip32Fast)

# NBip32Fast
*High perfomance BIP-32 HD key derivation library for .NET 8*

## Usage
### Basic
```cs
var secp256k1Key = NBip32Fast.Derivation.Secp256K1.DerivePath("m/44'/0'/0'/0/0", seed).Key;
var ed25519Key = NBip32Fast.Derivation.Ed25519.DerivePath("m/44'/0'/0'/0'/0'", seed).Key;
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
| Method        | Mean      | Error     | StdDev   |
|-------------- |----------:|----------:|---------:|
| NBitcoinKey   | 705.46 us |  5.886 us | 5.218 us |
| **NBip39FastKey**|  57.82 us |  0.278 us | 0.217 us |
| NetezosKey    | 977.09 us | 12.330 us | 9.627 us |

### Ed25519
| Method                  | Mean     | Error     | StdDev    |
|------------------------ |---------:|----------:|----------:|
| P3HdKey                 | 9.932 us | 0.1545 us | 0.1290 us |
| **NBip32FastKey**       | 7.126 us | 0.0319 us | 0.0266 us |
| NetezosKey              | 9.242 us | 0.0867 us | 0.0677 us |

[Benchmark code](https://github.com/kzorin52/NBip32Fast/blob/master/NBip32Fast.Benchmark/Program.cs)
