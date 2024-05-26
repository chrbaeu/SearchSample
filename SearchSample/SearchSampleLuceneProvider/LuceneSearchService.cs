using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SearchSample.QueryParser;
using SearchSample.RequestModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Directory = Lucene.Net.Store.Directory;

namespace SearchSampleLuceneProvider;

public class LuceneSearchService : IDisposable, ILuceneSearchService
{
    public SearchQueryParser SearchQueryParser { get; }

    private readonly Directory directory;
    private readonly Analyzer analyzer;
    private readonly IndexWriter writer;
    private readonly LuceneQueryConditionBuilder luceneQueryConditionBuilder;

    public LuceneSearchService() : this(new SearchQueryParser(), new RAMDirectory()) { }
    public LuceneSearchService(SearchQueryParser searchQueryParser) : this(searchQueryParser, new RAMDirectory()) { }

    public LuceneSearchService(DirectoryInfo directoryInfo) : this(new SearchQueryParser(), FSDirectory.Open(directoryInfo.FullName)) { }
    public LuceneSearchService(SearchQueryParser searchQueryParser, DirectoryInfo directoryInfo) : this(searchQueryParser, FSDirectory.Open(directoryInfo.FullName)) { }

    private LuceneSearchService(SearchQueryParser searchQueryParser, Directory directory)
    {
        this.SearchQueryParser = searchQueryParser;
        this.directory = directory;
        analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48, CharArraySet.EMPTY_SET);
        writer = new IndexWriter(this.directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer));
        luceneQueryConditionBuilder = new(((SearchQueryParser)SearchQueryParser).TokenizerConfig, [
                (nameof(SearchableDocument.Title), 2),
                (nameof(SearchableDocument.FullText), 1)
            ]);
    }

    public void Add(SearchableDocument searchableDocument)
    {
        var doc = MapToDocument(searchableDocument);
        writer.AddDocument(doc);
        writer.Commit();
    }

    public void Add(IEnumerable<SearchableDocument> searchableDocuments)
    {
        foreach (var doc in searchableDocuments)
        {
            writer.AddDocument(MapToDocument(doc));
        }
        writer.Commit();
    }

    public bool Exists(Guid uuid)
    {
        var term = new Term(nameof(SearchableDocument.Uuid), uuid.ToString());
        using var reader = DirectoryReader.Open(directory);
        var searcher = new IndexSearcher(reader);
        var query = new TermQuery(term);
        var hits = searcher.Search(query, 1).ScoreDocs;
        return hits.Length > 0;
    }

    public void Update(SearchableDocument searchableDocument)
    {
        var term = new Term(nameof(SearchableDocument.Uuid), searchableDocument.Uuid.ToString());
        var doc = MapToDocument(searchableDocument);
        writer.UpdateDocument(term, doc);
        writer.Commit();
    }

    public void Update(IEnumerable<SearchableDocument> searchableDocuments)
    {
        foreach (var doc in searchableDocuments)
        {
            var term = new Term(nameof(SearchableDocument.Uuid), doc.Uuid.ToString());
            writer.UpdateDocument(term, MapToDocument(doc));
        }
        writer.Commit();
    }

    public void Delete(Guid uuid)
    {
        var term = new Term(nameof(SearchableDocument.Uuid), uuid.ToString());
        writer.DeleteDocuments(term);
        writer.Commit();
    }

    public void Delete(IEnumerable<Guid> uuids)
    {
        foreach (var uuid in uuids)
        {
            var term = new Term(nameof(SearchableDocument.Uuid), uuid.ToString());
            writer.DeleteDocuments(term);
        }
        writer.Commit();
    }

    public IList<Guid> FindUuids(SearchRequest searchRequest, int maxResults = 100)
    {
        var tokens = SearchQueryParser.ParseToPostfixTokens(searchRequest.SearchQuery);
        var query = luceneQueryConditionBuilder.ConvertToLuceneQuery(tokens);
        query = ApplyFiltersToQuery(query, searchRequest.SearchFilters);
        if (query is null) { return []; }
        return FindUuids(query, maxResults);
    }

    public IList<SearchableDocument> FindSearchableDocuments(SearchRequest searchRequest, int maxResults = 100)
    {
        var tokens = SearchQueryParser.ParseToPostfixTokens(searchRequest.SearchQuery);
        var query = luceneQueryConditionBuilder.ConvertToLuceneQuery(tokens);
        query = ApplyFiltersToQuery(query, searchRequest.SearchFilters);
        if (query is null) { return []; }
        return FindSearchableDocuments(query, maxResults);
    }

    public IList<Guid> FindUuids(Query query, int maxResults = 100)
    {
        using var reader = DirectoryReader.Open(directory);
        var searcher = new IndexSearcher(reader);
        var hits = searcher.Search(query, maxResults).ScoreDocs;
        return hits.Select(hit => Guid.Parse(searcher.Doc(hit.Doc).Get(nameof(SearchableDocument.Uuid)))).ToList();
    }

    public IList<SearchableDocument> FindSearchableDocuments(Query query, int maxResults = 100)
    {
        using var reader = DirectoryReader.Open(directory);
        var searcher = new IndexSearcher(reader);
        var hits = searcher.Search(query, maxResults).ScoreDocs;
        return hits.Select(hit => MapToSearchableDocument(searcher.Doc(hit.Doc))).ToList();
    }

    public IList<Guid> GetAllKeys()
    {
        try
        {
            using var reader = DirectoryReader.Open(directory);
            var searcher = new IndexSearcher(reader);
            var hits = searcher.Search(new MatchAllDocsQuery(), int.MaxValue).ScoreDocs;
            return hits.Select(hit => Guid.Parse(searcher.Doc(hit.Doc).Get(nameof(SearchableDocument.Uuid)))).ToList();
        }
        catch (IndexNotFoundException)
        {
            return [];
        }
    }

    public IList<SearchableDocument> GetAllDocuments()
    {
        try
        {
            using var reader = DirectoryReader.Open(directory);
            var searcher = new IndexSearcher(reader);
            var hits = searcher.Search(new MatchAllDocsQuery(), int.MaxValue).ScoreDocs;
            return hits.Select(hit => MapToSearchableDocument(searcher.Doc(hit.Doc))).ToList();
        }
        catch (IndexNotFoundException)
        {
            return [];
        }
    }

    public void Dispose()
    {
        writer?.Dispose();
        analyzer?.Dispose();
        directory?.Dispose();
    }

    private Query? ApplyFiltersToQuery(Query? baseQuery, IEnumerable<SearchFilter> filters)
    {
        if (filters == null || !filters.Any()) { return baseQuery; }
        var booleanQuery = new BooleanQuery();
        if (baseQuery is not null)
        {
            booleanQuery.Add(baseQuery, Occur.MUST);
        }
        foreach (var filter in filters)
        {
            if (filter.Values != null && filter.Values.Count != 0)
            {
                var filterQuery = new BooleanQuery();
                foreach (var value in filter.Values)
                {
                    var termQuery = new TermQuery(new Term($"FilterableItem_{filter.Category}", value));
                    filterQuery.Add(termQuery, Occur.SHOULD);
                }
                booleanQuery.Add(filterQuery, Occur.MUST);
            }
        }
        return booleanQuery;
    }

    private Document MapToDocument(SearchableDocument obj)
    {
        var doc = new Document
        {
            new StringField(nameof(SearchableDocument.Uuid), obj.Uuid.ToString(), Field.Store.YES),
            new TextField(nameof(SearchableDocument.Title), obj.Title, Field.Store.YES),
            new TextField(nameof(SearchableDocument.FullText), obj.FullText, Field.Store.YES)
        };
        foreach (var item in obj.FilterableItems)
        {
            doc.Add(new StringField($"FilterableItem_{item.Category}", item.Value, Field.Store.YES));
        }
        return doc;
    }

    private SearchableDocument MapToSearchableDocument(Document doc)
    {
        var uuid = Guid.Parse(doc.Get(nameof(SearchableDocument.Uuid)));
        var title = doc.Get(nameof(SearchableDocument.Title));
        var fullText = doc.Get(nameof(SearchableDocument.FullText));
        var filterableItems = doc.Fields
            .Where(f => f.Name.StartsWith("FilterableItem_"))
            .Select(f => (FilterableItem)new FilterableItem
            {
                Category = f.Name["FilterableItem_".Length..],
                Value = f.GetStringValue()
            })
            .ToList();
        return new SearchableDocument
        {
            Uuid = uuid,
            Title = title,
            FullText = fullText,
            FilterableItems = filterableItems
        };
    }

}
