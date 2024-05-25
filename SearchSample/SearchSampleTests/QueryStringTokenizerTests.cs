using FluentAssertions;
using SearchSample.QueryParser;
using System;

namespace SearchSampleTests;

public class QueryStringTokenizerTests
{
    private readonly QueryStringTokenizer stringTokenizer = new(new TokenizerConfig());

    [Theory]
    [InlineData("", "")]
    [InlineData("Test", "Test")]
    [InlineData("Test1 Test2", "Test1_&_Test2")]
    [InlineData("Test1 & Test2", "Test1_&_Test2")]
    [InlineData("Test1 | Test2", "Test1_|_Test2")]
    [InlineData("Test1 & \"Test2 Test3\"", "Test1_&_Test2 Test3")]
    [InlineData("Test1 & (Test2 | Test3)", "Test1_&_(_Test2_|_Test3_)")]
    public void GetTokensTest(string query, string result)
    {
        var tokens = stringTokenizer.GetTokens(query);

        tokens.Should().BeEquivalentTo(result.Split('_', StringSplitOptions.RemoveEmptyEntries));
    }
}
