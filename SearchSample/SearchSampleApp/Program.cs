using Microsoft.EntityFrameworkCore;
using SearchSample.DataProvider;
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
        SimpleItem[] items = ["Item Rot Blau Schwarz", "Item Rot Grün Blaugrau", "Item Gelb Grün Rot", "Item Rot Blau Grün", "Item Blau Schwarz Geld"];

        TokenizerConfig config = new();

        QueryStringTokenizer tokenizer = new(config);
        InfixToPostfixConverter converter = new(config);
        var tokens = tokenizer.GetTokens(query);
        var postfix = converter.InfixToPostfix(tokens);

        LinqPredicateBuilder predicateBuilder = new(config, StringComparison.OrdinalIgnoreCase);
        var predicate = predicateBuilder.CreateExpression(postfix, (SimpleItem x) => x.Text);
        var compiledPredicate = predicate.Compile();

        var result = items.Where(compiledPredicate).ToArray();

        SearchWordHighlighter highlighter = new(config, postfix, false);
        Console.WriteLine($"{string.Join(", ", result.Select(x => highlighter.HighlightText(x.Text)))}");

        highlighter = new(config, postfix, true);
        Console.WriteLine($"{string.Join(", ", result.Select(x => highlighter.HighlightText(x.Text)))}");


        var searchRequest = new SearchRequest("senkrecht bis") { SearchFilters = [new SearchFilter(Guid.Empty, "B")] };

        RunInMemorySample(config, searchRequest);

        RunSqliteDbSample(config, searchRequest);

    }

    private static void RunInMemorySample(TokenizerConfig config, SearchRequest searchRequest)
    {
        SearchService<SearchableData, IReadOnlyCollection<FilterTag>> searchService = new(config, new LinqPredicateBuilder(config, StringComparison.Ordinal));

        var data = JsonSerializer.Deserialize<List<SearchableData>>(File.ReadAllText("items.json"))!;

        DictionaryBasedSearchDataProvider searchDataProvider = new();
        searchDataProvider.SetItems(data);

        var timestamp = Stopwatch.GetTimestamp();

        var searchResult = searchService.WeightedSearch(searchDataProvider, searchRequest);

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time (InMemory): {time.Milliseconds}ms Matches: {searchResult.Count}");
    }

    private static void RunSqliteDbSample(TokenizerConfig config, SearchRequest searchRequest)
    {
        SearchService<SearchableDataDo, List<FilterTagDo>> searchService = new(config, new SqlitePredicateBuilder(config));
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

    private static void InitSqlServerDb(TokenizerConfig config, SearchRequest searchRequest)
    {
        SearchService<SearchableDataDo, List<FilterTagDo>> searchService = new(config, new SqlitePredicateBuilder(config));
        var options = new DbContextOptionsBuilder<SearchSampleDbContext>()
            .UseSqlServer("Server=(LocalDb)\\MSSQLLocalDB;Database=BKI_DB_V1.8.4_Dev;Integrated Security=SSPI")
            .Options;
        using SearchSampleDbContext myDbContext = new(options);
        myDbContext.Database.EnsureCreated();

        var data = JsonSerializer.Deserialize<List<SearchableDataDo>>(File.ReadAllText("items.json"))!;

        DbContextBasedSearchDataProvider searchDataProvider = new(myDbContext);
        searchDataProvider.SetItems(data);

        var timestamp = Stopwatch.GetTimestamp();

        var searchResult = searchService.WeightedSearch(searchDataProvider, searchRequest);

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time (SqlServer): {time.Milliseconds}ms Matches: {searchResult.Count}");
    }

    public record class SqlServerData(string Uuid, string PositionNumber, string Description, string DescriptionLong, string RecordedUnit) { }

    public class SimpleItem
    {
        public string Text { get; set; } = "";

        public static implicit operator string(SimpleItem test) => test.Text;

        public static implicit operator SimpleItem(string text) => new() { Text = text };
    }
}
