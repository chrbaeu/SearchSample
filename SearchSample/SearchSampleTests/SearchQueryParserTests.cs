using FluentAssertions;
using SearchSample.QueryParser;
using System;

namespace SearchSampleTests;
public class SearchQueryParserTests
{
    private readonly SearchQueryParser searchQueryParser = new();

    [Theory]
    [InlineData("", "")]
    [InlineData("Test", "Test")]
    [InlineData("Test1 Test2", "Test1_Test2")]
    [InlineData("Test1 & (Test2 | Test3) & !Test4", "Test1_Test2_Test3")]
    public void GetSearchWordsTest(string query, string result)
    {
        var tokens = searchQueryParser.GetSearchWords(query);

        tokens.Should().BeEquivalentTo(result.Split('_', StringSplitOptions.RemoveEmptyEntries));
    }
}
