
using System;
using System.Collections.Generic;

namespace SearchSample.SearchProcessing;

public interface ISearchableData<TFilterTagCollection> where TFilterTagCollection : IEnumerable<ISearchFilterData>
{
    Guid ItemUuid { get; }
    string FullText { get; }
    TFilterTagCollection FilterTags { get; }
}
