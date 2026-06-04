using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;

namespace MyBenchmarks
{
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class Md5VsSha256
    {
        private const int N = 10000;
        private readonly byte[] data;

        private readonly SHA256 sha256 = SHA256.Create();
        private readonly MD5 md5 = MD5.Create();

        public Md5VsSha256()
        {
            data = new byte[N];
            new Random(42).NextBytes(data);
        }

        [Benchmark ]
        public byte[] Sha256() => sha256.ComputeHash(data);

        [Benchmark]
        public byte[] Md5() => md5.ComputeHash(data);
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner .Run<Md5VsSha256>();
        }
    }
}
