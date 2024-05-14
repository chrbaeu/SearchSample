using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SearchSample.DataProvider;
using SearchSample.QueryParser;
using SearchSample.QueryProcessing;
using SearchSample.SearchProcessing;
using SearchSampleApp.DbDataProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SearchSampleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var query = "Rot & Grün (Blau | Gelb)";
        SimpleItem[] items = ["Item Rot Blau Schwarz", "Item Rot Grün Blaugrau", "Item Gelb Grün Rot",
            "Item Rot Blau Grün", "Item Blau Schwarz Geld", "Item Red Green Blau"];

        SearchQueryParser searchQueryParser = new();
        searchQueryParser.SynonymHandler.AddSynonym("Rot", "Red");
        searchQueryParser.SynonymHandler.AddSynonym("Grün", "Green");
        var postfixTokens = searchQueryParser.ParseToPostfixTokens(query);

        LinqPredicateBuilder predicateBuilder = new(searchQueryParser.TokenizerConfig);
        var predicate = predicateBuilder.CreateExpression(postfixTokens, (SimpleItem x) => x.Text);
        var compiledPredicate = predicate.Compile();

        var result = items.Where(compiledPredicate).ToArray();

        SearchWordHighlighter highlighter = new(searchQueryParser.GetSearchWords(query), false);
        Console.WriteLine($"{string.Join(", ", result.Select(x => highlighter.HighlightText(x.Text)))}");

        highlighter = new(searchQueryParser.GetSearchWords(query), true);
        Console.WriteLine($"{string.Join(", ", result.Select(x => highlighter.HighlightText(x.Text)))}");


        var searchRequest = new SearchRequest("senkrecht bis");// { SearchFilters = [new SearchFilter(Guid.Empty, "B")] };

        RunInMemorySample(searchQueryParser, searchRequest);

        RunSqliteDbSample(searchQueryParser, searchRequest);

        //RunSqlServerDb(searchQueryParser, searchRequest);
    }

    private static void RunInMemorySample(SearchQueryParser config, SearchRequest searchRequest)
    {
        SearchService<SearchableData, IReadOnlyCollection<FilterTag>> searchService = new(config);

        var data = JsonSerializer.Deserialize<List<SearchableData>>(File.ReadAllText("items.json"))!;
        //File.WriteAllText("items.json", JsonSerializer.Serialize(data));

        DictionaryBasedSearchDataProvider searchDataProvider = new();
        searchDataProvider.SetItems(data);

        var timestamp = Stopwatch.GetTimestamp();

        var searchResult = searchService.WeightedSearch(searchDataProvider, searchRequest);

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time (InMemory): {time.Milliseconds}ms Matches: {searchResult.Count}");
    }

    private static void RunSqliteDbSample(SearchQueryParser config, SearchRequest searchRequest)
    {
        QueryableSearchExtensions.AddPredicateBuilder<EntityQueryProvider, SqlitePredicateBuilder>();

        SearchService<SearchableDataDo, List<FilterTagDo>> searchService = new(config);
        File.Delete("TestDb.db");
        var options = new DbContextOptionsBuilder<SearchSampleDbContext>()
            .UseSqlite("Data Source=TestDb.db")
            .EnableSensitiveDataLogging()
            .Options;
        using SearchSampleDbContext myDbContext = new(options);
        myDbContext.Database.EnsureCreated();

        var data = JsonSerializer.Deserialize<List<SearchableDataDo>>(File.ReadAllText("items.json"))!;

        DbContextBasedSearchDataProvider searchDataProvider = new(myDbContext);
        searchDataProvider.SetItems(data);

        var timestamp = Stopwatch.GetTimestamp();

        var searchResult = searchService.WeightedSearch(searchDataProvider, searchRequest);

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time (Sqlite): {time.Milliseconds}ms Matches: {searchResult.Count}");
    }

    private static void RunSqlServerDb(SearchQueryParser config, SearchRequest searchRequest)
    {
        SearchService<SearchableDataDo, List<FilterTagDo>> searchService = new(config);
        var options = new DbContextOptionsBuilder<SearchSampleDbContext>()
            .UseSqlServer("")
            .Options;
        using SearchSampleDbContext myDbContext = new(options);
        myDbContext.Database.EnsureCreated();

        var data = JsonSerializer.Deserialize<List<SearchableDataDo>>(File.ReadAllText("items.json"))!;

        DbContextBasedSearchDataProvider searchDataProvider = new(myDbContext);
        searchDataProvider.SetItems(data);

        var items = myDbContext.SearchableData.ToArray();

        var timestamp = Stopwatch.GetTimestamp();

        var searchResult = searchService.Search(searchDataProvider, searchRequest);

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time (SqlServer): {time.Milliseconds}ms Matches: {searchResult.Count}");

    }

    public class SimpleItem
    {
        public string Text { get; set; } = "";

        public static implicit operator string(SimpleItem test) => test.Text;

        public static implicit operator SimpleItem(string text) => new() { Text = text };
    }
}
