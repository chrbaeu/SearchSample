using SearchSample.RequestModels;
using System;
using System.Collections.Generic;

namespace SearchSampleLuceneProvider;

public interface ILuceneSearchService : IDisposable
{
    public void Add(SearchableDocument searchableDocument);
    public void Add(IEnumerable<SearchableDocument> searchableDocuments);
    public void Update(SearchableDocument searchableDocument);
    public void Update(IEnumerable<SearchableDocument> searchableDocuments);
    public void Delete(Guid uuid);
    public void Delete(IEnumerable<Guid> uuids);
    public IList<Guid> FindKeys(SearchRequest searchRequest, int maxResults = 100);
    public IList<SearchableDocument> FindDocuments(SearchRequest searchRequest, int maxResults = 100);
}
