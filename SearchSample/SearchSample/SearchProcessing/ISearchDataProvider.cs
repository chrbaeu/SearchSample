namespace SearchSample.SearchProcessing;

public interface ISearchDataProvider<TSearchableData, TFilterTagCollection>
    where TSearchableData : ISearchableData<TFilterTagCollection>
    where TFilterTagCollection : IEnumerable<IFilterTag>
{
    IQueryable<TSearchableData> GetQueryable();
}
