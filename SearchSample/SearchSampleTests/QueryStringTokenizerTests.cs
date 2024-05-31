using FluentAssertions;
using SearchSample.QueryParser;
using System;

namespace SearchSampleTests;

public class QueryStringTokenizerTests
{
    private readonly QueryStringTokenizer queryStringTokenizer = new(new TokenizerConfig());

    [Theory]
    [InlineData("Test", "Test")]
    [InlineData("Test1 Test2", "Test1_&_Test2")]
    [InlineData("Test1 & Test2", "Test1_&_Test2")]
    [InlineData("Test1 | Test2", "Test1_|_Test2")]
    [InlineData("Test1 | ''", "Test1")]
    [InlineData("Test1 & 'Test2 Test3'", "Test1_&_\"Test2 Test3\"")]
    [InlineData("Test1 & \"Test2 Test3\"", "Test1_&_\"Test2 Test3\"")]
    [InlineData("Test1 & (Test2 | Test3)", "Test1_&_(_Test2_|_Test3_)")]
    [InlineData("[Test1 | Test2] Test3", "(_Test1_|_Test2_)_&_Test3")]
    [InlineData("{Test1 | Test2} | Test3", "(_Test1_|_Test2_)_|_Test3")]
    [InlineData("Test1 & (Test2 | Test3) & !Test4", "Test1_&_(_Test2_|_Test3_)_&_!_Test4")]
    [InlineData("Test1 & & & Test2", "Test1_&_Test2")]
    [InlineData("Test1 & & ! Test2", "Test1_&_!_Test2")]
    [InlineData(") Test1 & & ! ! Test2 & ! ( !", "Test1_&_!_Test2")]
    [InlineData("Test1 and Test2", "Test1_&_Test2")]
    [InlineData("Test1 or Test2", "Test1_|_Test2")]
    [InlineData("Test1 '' Test2", "Test1_&_Test2")]
    [InlineData("'Test1 Test2", "\"Test1 Test2\"")]
    [InlineData("\\'Test1 Test2", "'Test1_&_Test2")]
    [InlineData("Test1 ( Test2 )", "Test1_&_Test2")]
    [InlineData("Test1 ( & )", "Test1")]
    [InlineData("Test1 ( ( & )", "Test1")]
    [InlineData("Test1 ( ( ( & )", "Test1")]
    [InlineData("Test1 ( ( ( & ) &  Test2", "Test1_&_Test2")]
    [InlineData("Test1 ( ( ( Test2 & ) ) ) Test3", "Test1_&_Test2_&_Test3")]
    [InlineData("Test1 ( ( ( ! ) ) ) Test3", "Test1_&_Test3")]
    [InlineData("( ( ( & ) &  Test2", "Test2")]
    [InlineData("Test1 (Test2 | 'Test3 Test4')", "Test1_&_(_Test2_|_\"Test3 Test4\"_)")]
    public void GetTokens_ShouldReturnTokens_ForValidQueries(string query, string result)
    {
        var tokens = queryStringTokenizer.GetTokens(query);

        tokens.Should().BeEquivalentTo(result.Split('_', StringSplitOptions.RemoveEmptyEntries));
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("(")]
    [InlineData(")")]
    [InlineData("&")]
    [InlineData("|")]
    [InlineData("!")]
    public void GetTokens_ShouldReturnEmptyList_ForInvalidQueries(string query)
    {
        var tokens = queryStringTokenizer.GetTokens(query);
        tokens.Should().BeEmpty();
    }

}
