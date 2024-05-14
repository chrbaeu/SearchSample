using BenchmarkDotNet.Attributes;
using SearchSample.QueryParser;
using SearchSample.QueryProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SearchSamplePerformanceTests;

public class OverviewBenchmark
{
    private static readonly TokenizerConfig config = new();

    private readonly QueryStringTokenizer tokenizer = new(config);
    private readonly InfixToPostfixConverter converter = new(config);
    private readonly LinqPredicateBuilder predicateBuilder = new(config);

    private readonly string query = "A & B (C | D)";
    private readonly Test[] data = ["ABB", "ABC", "ABD", "ABE", "XBC", "ABB", "ABC", "ABD", "ABE", "XBC"];

    private readonly List<string> tokens;
    private readonly List<string> postfix;
    private readonly Expression<Func<Test, bool>> predicate;
    private readonly Func<Test, bool> compiledPredicate;

    public OverviewBenchmark()
    {
        tokens = tokenizer.GetTokens(query);
        postfix = converter.InfixToPostfix(tokens).ToList();
        predicate = predicateBuilder.CreateExpression(postfix, (Test x) => x.Text);
        compiledPredicate = predicate.Compile();
    }

    [Benchmark]
    public List<string> GetTokens()
    {
        return tokenizer.GetTokens(query);
    }

    [Benchmark]
    public List<string> InfixToPostfix()
    {
        return converter.InfixToPostfix(tokens).ToList();
    }

    [Benchmark]
    public Expression<Func<Test, bool>> CreateExpression()
    {
        return predicateBuilder.CreateExpression(postfix, (Test x) => x.Text);
    }

    [Benchmark]
    public Func<Test, bool> Compile()
    {
        return predicate.Compile();
    }

    [Benchmark]
    public Test[] Where()
    {
        return data.Where(compiledPredicate).ToArray();
    }

    public class Test
    {
        public string Text { get; set; } = "";

        public static implicit operator string(Test test)
        {
            return test.Text;
        }
        public static implicit operator Test(string text)
        {
            return new Test { Text = text };
        }
    }

}
