using SearchSample.SearchProcessing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchSample.DataProvider;

public class DictionaryBasedSearchDataProvider : ISearchDataProvider<SearchableData, IReadOnlyCollection<FilterTag>>
{

    private readonly Dictionary<Guid, SearchableData> dataDict = [];

    public IQueryable<SearchableData> GetQueryable()
    {
        return dataDict.Values.AsQueryable();
    }

    public void SetItems(ICollection<SearchableData> searchableData)
    {
        foreach (var item in searchableData)
        {
            dataDict[item.ItemUuid] = item;
        }
    }

    public void SetItem(SearchableData searchableData)
    {
        dataDict[searchableData.ItemUuid] = searchableData;
    }

    public void RemoveItem(Guid uuid)
    {
        dataDict.Remove(uuid);
    }

}
