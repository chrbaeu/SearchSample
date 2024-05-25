using Microsoft.EntityFrameworkCore;

namespace SearchSampleApp.DbDataProvider;

public class SearchSampleDbContext : DbContext
{
    public DbSet<SearchableDataDo> SearchableData { get; set; }
    public DbSet<FilterTagDo> FilterTags { get; set; }

    public SearchSampleDbContext(DbContextOptions<SearchSampleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchableDataDo>()
            .HasKey(s => s.Uuid);

        modelBuilder.Entity<SearchableDataDo>()
            .Property(s => s.Uuid)
            .IsRequired();

        modelBuilder.Entity<SearchableDataDo>()
            .HasMany(s => s.SearchFilters)
            .WithOne()
            .HasForeignKey(f => f.ItemUuid);

        modelBuilder.Entity<FilterTagDo>()
            .HasKey(f => f.Id);

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.ItemUuid)
            .IsRequired();

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.Category)
            .IsRequired();

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.Value)
            .IsRequired();
    }
}
