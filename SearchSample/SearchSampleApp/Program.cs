using Microsoft.EntityFrameworkCore;
using SearchSample.DataProvider;
using SearchSample.QueryProcessing;
using SearchSample.SearchProcessing;
using System.Diagnostics;

namespace SearchSampleApp;

internal class Program
{
    static void MainX(string[] args)
    {
        TokenizerConfig config = new();
        QueryStringTokenizer tokenizer = new(config);
        InfixToPostfixConverter converter = new(config);
        SqlQueryConditionBuilder sqlQueryConditionBuilder = new(config);
        PredicateBuilder predicateBuilder = new(config);
        var query = "A & B (C | D)";
        var tokens = tokenizer.GetTokens(query);
        var postfix = converter.InfixToPostfix(tokens);
        var sqlCondition = sqlQueryConditionBuilder.ConvertToSql(postfix, "Text");
        Console.WriteLine($"SELECT * FROM Test WHERE {sqlCondition}");

        var predicate = predicateBuilder.CreateExpression(postfix, (Test x) => x.Text);
        var compiledPredicate = predicate.Compile();
        Test[] data = ["ABB", "ABC", "ABD", "ABE", "XBC"];
        var result = data.Where(compiledPredicate).ToArray();
        Console.WriteLine($"Result: {string.Join(", ", result.Select(x => x.Text))}");
    }

    static void Main(string[] args)
    {
        TokenizerConfig config = new();
        SearchService<SearchableDataDo, List<FilterTagDo>> searchService = new(config);
        //File.Delete("TestDb.db");
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseSqlite("Data Source=TestDb.db")
            .Options;
        using MyDbContext myDbContext = new(options);
        myDbContext.Database.EnsureCreated();

        DbContextBasedSearchDataProvider searchDataProvider = new(myDbContext);


        var timestamp = Stopwatch.GetTimestamp();

        var result = searchService.WeightedSearch(searchDataProvider, new SearchRequest("senkrecht bis (graben | verbau)") { FilterTag = [new FilterTag(Guid.Empty, "B")] });

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time: {time.Milliseconds}ms");
    }

    static void MainY(string[] args)
    {
        TokenizerConfig config = new();
        SearchService<SearchableData, IReadOnlyCollection<FilterTag>> searchService = new(config);
        DictionaryBasedSearchDataProvider searchDataProvider = new();


        var timestamp = Stopwatch.GetTimestamp();

        var result = searchService.WeightedSearch(searchDataProvider, new SearchRequest("senkrecht bis"));

        var time = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Time: {time.Milliseconds}ms");
    }


    public class Test
    {
        public string Text { get; set; }

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
