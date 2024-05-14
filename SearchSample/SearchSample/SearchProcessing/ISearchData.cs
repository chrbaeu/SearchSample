
using System;
using System.Collections.Generic;

namespace SearchSample.SearchProcessing;

public interface ISearchData<TFilterTagCollection> where TFilterTagCollection : IEnumerable<ISearchFilterData>
{
    Guid SourceUuid { get; }
    string FullText { get; }
    TFilterTagCollection FilterTags { get; }
}
