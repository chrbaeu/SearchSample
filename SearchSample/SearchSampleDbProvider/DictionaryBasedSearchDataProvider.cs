using SearchSample.SearchProcessing;
using SearchSampleApp.DbDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchSample.DataProvider;

public class DictionaryBasedSearchDataProvider2 : ISearchDataProvider<SearchableDataDo, List<FilterTagDo>>
{

    private readonly Dictionary<Guid, SearchableDataDo> dataDict = [];

    public IQueryable<SearchableDataDo> GetQueryable()
    {
        return dataDict.Values.AsQueryable();
    }

    public void SetItems(ICollection<SearchableDataDo> searchableData)
    {
        foreach (var item in searchableData)
        {
            dataDict[item.SourceUuid] = item;
        }
    }

    public void SetItem(SearchableDataDo searchableData)
    {
        dataDict[searchableData.SourceUuid] = searchableData;
    }

    public void RemoveItem(Guid uuid)
    {
        dataDict.Remove(uuid);
    }

}
