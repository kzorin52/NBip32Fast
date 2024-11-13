using BenchmarkDotNet.Running;
using NBip32Fast;
using NBip32Fast.Benchmark;
using NBip32Fast.Ed25519;
using NBip32Fast.NistP256;
using NBip32Fast.Secp256K1;
using Nethermind.Crypto;
using NistP256Net;

//var test = new Secp256K1Tests();
//Console.WriteLine(Convert.ToHexStringLower(test.NBitcoinKey()));
//Console.WriteLine(Convert.ToHexStringLower(test.NBip39FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test.NetezosKey()));

//BenchmarkRunner.Run<Secp256K1Tests>();

//var test2 = new Ed25519Tests();
//Console.WriteLine(Convert.ToHexStringLower(test2.P3HdKey()));
//Console.WriteLine(Convert.ToHexStringLower(test2.NBip32FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test2.NetezosKey()));


var tests = new ParseKeyPath();
Console.WriteLine(tests.NBip32FastParse());
Console.WriteLine(tests.NBitcoinParse());
Console.WriteLine(tests.NetezosParse());

Console.ReadLine();
BenchmarkRunner.Run<ParseKeyPath>();


//var test3 = new Secp256R1Tests();
//Console.WriteLine(Convert.ToHexStringLower(test3.NBip39FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test3.NetezosKey()));
//BenchmarkRunner.Run<Secp256R1Tests>();

//var seed = Convert.FromHexString("52010ce3e2697a0fdf7eb871a52a46de4e254609d51a7c01dd93c57b04f4bb3bbe5fbca79d40543fbc3e18d2971ddc0a34fea5e81197865a108a0d82da28f05f");
//var nb = ExtKey.CreateFromSeed(seed);
//var der = nb.Derive(0, true);
//Console.WriteLine(Convert.ToHexStringLower(der.PrivateKey.ToBytes()));


//var key = new Bip32Key();
//NBip32Fast.Secp256K1.Secp256K1HdKey.Instance.GetMasterKeyFromSeed(seed, ref key);
//NBip32Fast.Secp256K1.Secp256K1HdKey.Instance.Derive(ref key, new KeyPathElement(0, true), ref key);

//Console.WriteLine(Convert.ToHexStringLower(key.Key));