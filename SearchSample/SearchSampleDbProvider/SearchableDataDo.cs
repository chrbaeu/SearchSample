using SearchSample.SearchProcessing;
using System;
using System.Collections.Generic;

namespace SearchSampleApp.DbDataProvider;

public class SearchableDataDo : ISearchableData<List<FilterTagDo>>
{
    public SearchableDataDo(Guid itemUuid, string fullText)
    {
        ItemUuid = itemUuid;
        FullText = fullText;
    }

    public Guid ItemUuid { get; set; } = Guid.Empty;
    public string FullText { get; set; } = "";
    public virtual List<FilterTagDo> FilterTags { get; set; } = [];
}
