﻿using BenchmarkDotNet.Running;

namespace SearchSamplePerformanceTests;

internal class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<CompareBenchmark>();
    }
}
