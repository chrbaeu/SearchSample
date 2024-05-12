using SearchSampleApp;

namespace SearchSample.SearchProcessing;

public class DbContextBasedSearchDataProvider : ISearchDataProvider<SearchableDataDo, List<FilterTagDo>>
{
    private readonly MyDbContext myDbContext;

    public DbContextBasedSearchDataProvider(MyDbContext myDbContext)
    {
        this.myDbContext = myDbContext;
    }

    public IQueryable<SearchableDataDo> GetQueryable()
    {
        return myDbContext.SearchableData.AsQueryable();
    }

    public void AddItemWithoutSaving(SearchableDataDo searchableData)
    {
        myDbContext.SearchableData.Add(searchableData);
    }

    public void SetItem(SearchableDataDo searchableData)
    {
        var item = myDbContext.SearchableData.FirstOrDefault(s => s.Uuid == searchableData.Uuid);
        if (item is null)
        {
            myDbContext.SearchableData.Add(searchableData);
        }
        else
        {
            myDbContext.SearchableData.Update(searchableData);
        }
        myDbContext.SaveChanges();
    }

    public void RemoveItem(Guid uuid)
    {
        var item = myDbContext.SearchableData.FirstOrDefault(s => s.Uuid == uuid);
        if (item is null) { return; }
        myDbContext.SearchableData.Remove(item);
        myDbContext.SaveChanges();
    }

}