using System;
using System.Collections.Generic;

namespace SearchSample.Interfaces;

public interface ISearchData<TSearchFilterData> where TSearchFilterData : IEnumerable<ISearchFilterData>
{
    Guid Uuid { get; }
    string FullText { get; }
    TSearchFilterData SearchFilters { get; }
}
