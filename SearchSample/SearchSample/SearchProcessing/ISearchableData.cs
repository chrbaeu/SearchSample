
using System;
using System.Collections.Generic;

namespace SearchSample.SearchProcessing;

public interface ISearchableData<TFilterTagCollection> where TFilterTagCollection : IEnumerable<IFilterTag>
{
    Guid ItemUuid { get; }
    string FullText { get; }
    TFilterTagCollection FilterTags { get; }
}
