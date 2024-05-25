using SearchSample.Interfaces;
using System;
using System.Collections.Generic;

namespace SearchSampleApp.DbDataProvider;

public record class SearchableDataDo : ISearchData<List<FilterTagDo>>
{
    public SearchableDataDo(Guid uuid, string fullText)
    {
        Uuid = uuid;
        FullText = fullText;
    }

    public Guid Uuid { get; set; } = Guid.Empty;
    public string FullText { get; set; } = "";
    public virtual List<FilterTagDo> SearchFilters { get; set; } = [];
}
