using SearchSample.SearchProcessing;

namespace SearchSample.DataProvider;

public class DictionaryBasedSearchDataProvider : ISearchDataProvider<SearchableData, IReadOnlyCollection<FilterTag>>
{

    private readonly Dictionary<Guid, SearchableData> dataDict = [];

    public IQueryable<SearchableData> GetQueryable()
    {
        return dataDict.Values.AsQueryable();
    }

    public void SetItem(SearchableData searchableData)
    {
        dataDict[searchableData.Uuid] = searchableData;
    }

    public void RemoveItem(Guid uuid)
    {
        dataDict.Remove(uuid);
    }

}