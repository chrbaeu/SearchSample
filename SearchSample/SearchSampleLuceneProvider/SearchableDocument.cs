using System;
using System.Collections.Generic;

namespace SearchSampleLuceneProvider;

public class SearchableDocument
{
    public Guid Uuid { get; set; } = Guid.Empty;
    public string Title { get; set; } = "";
    public string FullText { get; set; } = "";
    public List<FilterableItem> FilterableItems { get; set; } = [];
}
