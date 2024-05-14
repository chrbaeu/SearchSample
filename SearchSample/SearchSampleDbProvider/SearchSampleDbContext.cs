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
            .HasKey(s => s.SourceUuid);

        modelBuilder.Entity<SearchableDataDo>()
            .Property(s => s.SourceUuid)
            .IsRequired();

        modelBuilder.Entity<SearchableDataDo>()
            .HasMany(s => s.FilterTags)
            .WithOne()
            .HasForeignKey(f => f.ItemUuid);

        modelBuilder.Entity<FilterTagDo>()
            .HasKey(f => f.Id);

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.ItemUuid)
            .IsRequired();

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.FilterType)
            .IsRequired();

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.Value)
            .IsRequired();
    }
}
