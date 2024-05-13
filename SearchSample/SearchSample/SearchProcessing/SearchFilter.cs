using System;
using System.Collections.Generic;

namespace SearchSample.SearchProcessing;
public record class SearchFilter
{
    public SearchFilter(Guid filterTypeUuid, string value)
    {
        FilterTypeUuid = filterTypeUuid;
        Values = [value];
    }

    public SearchFilter(Guid filterTypeUuid, List<string> values)
    {
        FilterTypeUuid = filterTypeUuid;
        Values = values;
    }

    public Guid FilterTypeUuid { get; init; }
    public IReadOnlyCollection<string> Values { get; init; }
}
