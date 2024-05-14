using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SearchSample.QueryParser;
using SearchSample.QueryProcessing;
using SearchSample.RequestModels;
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

        var result = items.AsQueryable().SearchByQueryString(x => x.Text, query, searchQueryParser).ToArray();

        SearchWordHighlighter highlighter = new(searchQueryParser.GetSearchWords(query), false);
        Console.WriteLine($"{string.Join(", ", result.Select(x => highlighter.HighlightText(x.Text)))}");

        highlighter = new(searchQueryParser.GetSearchWords(query), true);
        Console.WriteLine($"{string.Join(", ", result.Select(x => highlighter.HighlightText(x.Text)))}");


        var searchRequest = new SearchRequest("senkrecht bis");

        RunInMemorySample(searchQueryParser, searchRequest);

        RunSqliteDbSample(searchQueryParser, searchRequest);

        //RunSqlServerDb(searchQueryParser, searchRequest);
    }

    private static void RunInMemorySample(SearchQueryParser searchQueryParser, SearchRequest searchRequest)
    {
        var data = JsonSerializer.Deserialize<List<SearchableDataDo>>(File.ReadAllText("items.json"))!;

        var timestamp = Stopwatch.GetTimestamp();

        var searchResult = data.AsQueryable().WeightedSearch<SearchableDataDo, List<FilterTagDo>>(searchRequest, searchQueryParser).ToList();

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time (InMemory): {time.Milliseconds}ms Matches: {searchResult.Count}");
    }

    private static void RunSqliteDbSample(SearchQueryParser searchQueryParser, SearchRequest searchRequest)
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        QueryableSearchExtensions.AddPredicateBuilder<EntityQueryProvider, SqlitePredicateBuilder>();
#pragma warning restore EF1001 // Internal EF Core API usage.

        File.Delete("TestDb.db");

        var options = new DbContextOptionsBuilder<SearchSampleDbContext>()
            .UseSqlite("Data Source=TestDb.db")
            .EnableSensitiveDataLogging()
            .Options;
        using SearchSampleDbContext ctx = new(options);
        ctx.Database.EnsureCreated();

        var data = JsonSerializer.Deserialize<List<SearchableDataDo>>(File.ReadAllText("items.json"))!;
        ctx.AddRange(data);
        ctx.SaveChanges();

        var timestamp = Stopwatch.GetTimestamp();

        var searchResult = ctx.SearchableData.AsQueryable().WeightedSearch<SearchableDataDo, List<FilterTagDo>>(searchRequest, searchQueryParser).ToList();

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time (Sqlite): {time.Milliseconds}ms Matches: {searchResult.Count}");
    }

    private static void RunSqlServerDb(SearchQueryParser searchQueryParser, SearchRequest searchRequest)
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        QueryableSearchExtensions.AddPredicateBuilder<EntityQueryProvider, SqlServerPredicateBuilder>();
#pragma warning restore EF1001 // Internal EF Core API usage.

        var options = new DbContextOptionsBuilder<SearchSampleDbContext>()
            .UseSqlServer("")
            .Options;
        using SearchSampleDbContext ctx = new(options);
        ctx.Database.EnsureCreated();

        var data = JsonSerializer.Deserialize<List<SearchableDataDo>>(File.ReadAllText("items.json"))!;
        ctx.AddRange(data);
        ctx.SaveChanges();

        var items = ctx.SearchableData.ToArray();

        var timestamp = Stopwatch.GetTimestamp();

        var searchResult = ctx.SearchableData.AsQueryable().WeightedSearch<SearchableDataDo, List<FilterTagDo>>(searchRequest, searchQueryParser).ToList();

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
