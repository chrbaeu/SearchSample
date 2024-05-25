using FluentAssertions;
using SearchSample.QueryParser;
using System;
using System.Linq;

namespace SearchSampleTests;
public class SearchQueryParserTests
{
    private readonly SearchQueryParser searchQueryParser = new();

    [Theory]
    [InlineData("", "")]
    [InlineData("Test", "Test")]
    [InlineData("Test1 Test2", "Test1_Test2_&")]
    [InlineData("Test1 & Test2", "Test1_Test2_&")]
    [InlineData("Test1 | Test2", "Test1_Test2_|")]
    [InlineData("Test1 | ''", "Test1")]
    [InlineData("Test1 & 'Test2 Test3'", "Test1_\"Test2 Test3\"_&")]
    [InlineData("Test1 | \"Test2 Test3\"", "Test1_\"Test2 Test3\"_|")]
    public void ParseToPostfixTokensTest(string query, string result)
    {
        var tokens = searchQueryParser.ParseToPostfixTokens(query);

        tokens.Should().BeEquivalentTo(result.Split('_', StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower()));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("Test", "Test")]
    [InlineData("Test1 Test2", "Test1_Test2")]
    [InlineData("Test1 & Test2", "Test1_Test2")]
    [InlineData("Test1 | Test2", "Test1_Test2")]
    [InlineData("Test1 | ''", "Test1")]
    [InlineData("Test1 & 'Test2 Test3'", "Test1_\"Test2 Test3\"")]
    [InlineData("Test1 & \"Test2 Test3\"", "Test1_\"Test2 Test3\"")]
    [InlineData("Test1 & (Test2 | Test3)", "Test1_Test2_Test3")]
    [InlineData("[Test1 | Test2] Test3", "Test1_Test2_Test3")]
    [InlineData("{Test1 | Test2} | Test3", "Test1_Test2_Test3")]
    [InlineData("Test1 & (Test2 | Test3) & !Test4", "Test1_Test2_Test3")]
    public void GetSearchWordsTest(string query, string result)
    {
        var tokens = searchQueryParser.GetSearchWords(query);

        tokens.Should().BeEquivalentTo(result.Split('_', StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower()));
    }

}
