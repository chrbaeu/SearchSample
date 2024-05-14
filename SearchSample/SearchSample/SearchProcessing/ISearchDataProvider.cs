using System.Collections.Generic;
using System.Linq;

namespace SearchSample.SearchProcessing;

public interface ISearchDataProvider<TSearchableData, TFilterTagCollection>
    where TSearchableData : ISearchData<TFilterTagCollection>
    where TFilterTagCollection : IEnumerable<ISearchFilter>
{
    IQueryable<TSearchableData> GetQueryable();
}
