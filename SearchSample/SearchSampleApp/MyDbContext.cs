using Microsoft.EntityFrameworkCore;

namespace SearchSampleApp;

public class MyDbContext : DbContext
{
    public DbSet<SearchableDataDo> SearchableData { get; set; }
    public DbSet<FilterTagDo> FilterTags { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
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
            .HasMany(s => s.FilterTags)
            .WithOne()
            .HasForeignKey(f => f.SearchableDataUuid);

        modelBuilder.Entity<FilterTagDo>()
            .HasKey(f => f.Id);

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.SearchableDataUuid)
            .IsRequired();

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.Type)
            .IsRequired();

        modelBuilder.Entity<FilterTagDo>()
            .Property(f => f.Value)
            .IsRequired();
    }
}
