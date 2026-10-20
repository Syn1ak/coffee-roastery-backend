using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Catalog;

namespace Shop.Infrastructure.Persistence.Configurations;

internal class CoffeeConfiguration : IEntityTypeConfiguration<Coffee>
{
    public void Configure(EntityTypeBuilder<Coffee> builder)
    {
        builder.ToTable("coffees");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.Name).HasColumnName("name");
        builder.Property(c => c.Origin).HasColumnName("origin");
        builder.Property(c => c.RoastStyle).HasColumnName("roast_style").HasConversion<string>();
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<string>();
        builder.Property(c => c.StockMode).HasColumnName("stock_mode").HasConversion<string>();

        builder.OwnsMany(c => c.Prices, p =>
        {
            p.ToTable("coffee_prices");
            p.WithOwner().HasForeignKey("CoffeeId");
            p.Property<Guid>("CoffeeId").HasColumnName("coffee_id");
            p.HasKey("CoffeeId", nameof(BagPrice.Size));
            p.Property(bp => bp.Size).HasColumnName("bag_size").HasConversion<string>();
            p.OwnsOne(bp => bp.Price, m =>
            {
                m.Property(money => money.Amount).HasColumnName("amount");
                m.Property(money => money.Currency).HasColumnName("currency");
            });
        });

        builder.Navigation(c => c.Prices).HasField("_prices");

        builder.Property<int>("Version").HasColumnName("version").IsConcurrencyToken();
    }
}
