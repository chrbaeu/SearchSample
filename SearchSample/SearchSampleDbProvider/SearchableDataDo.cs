using SearchSample.Interfaces;
using System;
using System.Collections.Generic;

namespace SearchSampleApp.DbDataProvider;

public record class SearchableDataDo : ISearchData<List<FilterTagDo>>
{
    public SearchableDataDo(Guid sourceUuid, string fullText)
    {
        SourceUuid = sourceUuid;
        FullText = fullText;
    }

    public Guid SourceUuid { get; set; } = Guid.Empty;
    public string FullText { get; set; } = "";
    public virtual List<FilterTagDo> FilterTags { get; set; } = [];
}
