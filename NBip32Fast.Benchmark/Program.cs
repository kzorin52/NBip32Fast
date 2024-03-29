using BenchmarkDotNet.Running;

//var test = new Secp256K1Tests();
//Console.WriteLine(Convert.ToHexStringLower(test.NBitcoinKey()));
//Console.WriteLine(Convert.ToHexStringLower(test.NBip39FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test.NetezosKey()));

BenchmarkRunner.Run<Secp256K1Tests>();

//var test2 = new Ed25519Tests();
//Console.WriteLine(Convert.ToHexStringLower(test2.P3HdKey()));
//Console.WriteLine(Convert.ToHexStringLower(test2.NBip32FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test2.NetezosKey()));

BenchmarkRunner.Run<Ed25519Tests>();
//BenchmarkRunner.Run<SerCacheTest>();

//var test3 = new Secp256R1Tests();
//Console.WriteLine(Convert.ToHexStringLower(test3.NBip39FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test3.NetezosKey()));
BenchmarkRunner.Run<Secp256R1Tests>();