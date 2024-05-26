using BenchmarkDotNet.Attributes;
using SearchSample.QueryParser;
using System.Collections.Generic;

namespace SearchSamplePerformanceTests;

public class CompareBenchmark
{
    private static readonly TokenizerConfig config = new();

    private readonly QueryStringTokenizerOld tokenizerOld = new(config);
    private readonly QueryStringTokenizer tokenizer = new(config);

    private readonly string queryShort = "A & B (C | D)";
    private readonly string queryLong = "A & B (C | D) & (AA & BB (CC | DD)) | 'HelloWorld' & 'A | B'";


    [Benchmark]
    public List<string> GetTokensOld_ShortQuery()
    {
        return tokenizerOld.GetTokens(queryShort);
    }

    [Benchmark]
    public List<string> GetTokens_ShortQuery()
    {
        return tokenizer.GetTokens(queryShort);
    }

    [Benchmark]
    public List<string> GetTokensOld_LongQuery()
    {
        return tokenizerOld.GetTokens(queryLong);
    }

    [Benchmark]
    public List<string> GetTokens_LongQuery()
    {
        return tokenizer.GetTokens(queryLong);
    }


}
