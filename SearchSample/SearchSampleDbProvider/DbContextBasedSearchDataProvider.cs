using SearchSample.SearchProcessing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchSampleApp.DbDataProvider;

public class DbContextBasedSearchDataProvider : ISearchDataProvider<SearchableDataDo, List<FilterTagDo>>
{
    private readonly SearchSampleDbContext myDbContext;

    public DbContextBasedSearchDataProvider(SearchSampleDbContext myDbContext)
    {
        this.myDbContext = myDbContext;
    }

    public IQueryable<SearchableDataDo> GetQueryable()
    {
        return myDbContext.SearchableData.AsQueryable();
    }

    public void SetItem(SearchableDataDo searchableData)
    {
        var item = myDbContext.SearchableData.FirstOrDefault(s => s.SourceUuid == searchableData.SourceUuid);
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

    public void SetItems(ICollection<SearchableDataDo> searchableData)
    {
        var uuids = searchableData.Select(x => x.SourceUuid);
        var existingItemDict = myDbContext.SearchableData.Where(s => uuids.Contains(s.SourceUuid)).ToDictionary(x => x.SourceUuid);
        foreach (var item in searchableData)
        {
            if (existingItemDict.TryGetValue(item.SourceUuid, out var existingItem))
            {
                myDbContext.SearchableData.Remove(existingItem);
                myDbContext.SearchableData.Update(item);
            }
            else
            {
                myDbContext.SearchableData.Add(item);
            }
        }
        myDbContext.SaveChanges();
    }

    public void RemoveItem(Guid uuid)
    {
        var item = myDbContext.SearchableData.FirstOrDefault(s => s.SourceUuid == uuid);
        if (item is null) { return; }
        myDbContext.SearchableData.Remove(item);
        myDbContext.SaveChanges();
    }

}
