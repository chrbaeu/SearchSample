using System.Collections.Generic;

namespace SearchSample.SearchProcessing;

public record class SearchFilter
{
    public SearchFilter() { }

    public SearchFilter(int filterType, long value)
    {
        FilterType = filterType;
        Values = [value];
    }

    public SearchFilter(int filterType, List<long> values)
    {
        FilterType = filterType;
        Values = values;
    }

    public int FilterType { get; init; }
    public IReadOnlyCollection<long> Values { get; init; } = [];
}
