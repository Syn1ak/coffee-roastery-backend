using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Catalog;

namespace Shop.Infrastructure.Persistence.Configurations;

internal class CoffeeConfiguration : IEntityTypeConfiguration<Coffee>
{
    public void Configure(EntityTypeBuilder<Coffee> builder)
    {
        builder.ToTable("coffees", t =>
        {
            t.HasCheckConstraint("coffees_roast_style_check", "roast_style IN ('Filter', 'Espresso')");
            t.HasCheckConstraint("coffees_status_check", "status IN ('Draft', 'Published', 'Retired')");
            t.HasCheckConstraint("coffees_stock_mode_check", "stock_mode IN ('Shelf', 'RoastToOrder')");
        });
        builder.HasKey(c => c.Id).HasName("coffees_pkey");
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever().HasDefaultValueSql("uuidv7()");
        builder.Property(c => c.Name).HasColumnName("name");
        builder.Property(c => c.Origin).HasColumnName("origin");
        builder.Property(c => c.RoastStyle).HasColumnName("roast_style").HasConversion<string>();
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<string>()
            .ValueGeneratedNever().HasDefaultValueSql("'Draft'");
        builder.Property(c => c.StockMode).HasColumnName("stock_mode").HasConversion<string>();

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("ix_coffees_published")
            .HasFilter("status = 'Published'");

        builder.OwnsMany(c => c.Prices, p =>
        {
            p.ToTable("coffee_prices", t =>
            {
                t.HasCheckConstraint("coffee_prices_bag_size_check", "bag_size IN ('Grams250', 'Kilogram1')");
                t.HasCheckConstraint("coffee_prices_amount_check", "amount > 0");
                t.HasCheckConstraint("coffee_prices_currency_check", "currency = 'EUR'");
            });
            p.WithOwner().HasForeignKey("CoffeeId").HasConstraintName("coffee_prices_coffee_id_fkey");
            p.Property<Guid>("CoffeeId").HasColumnName("coffee_id");
            p.HasKey("CoffeeId", nameof(BagPrice.Size)).HasName("coffee_prices_pkey");
            p.Property(bp => bp.Size).HasColumnName("bag_size").HasConversion<string>();
            p.OwnsOne(bp => bp.Price, m =>
            {
                m.Property(money => money.Amount).HasColumnName("amount").HasPrecision(10, 2);
                m.Property(money => money.Currency).HasColumnName("currency").HasMaxLength(3).IsFixedLength();
            });
        });

        builder.Navigation(c => c.Prices).HasField("_prices");

        builder.Property<int>("Version").HasColumnName("version").IsConcurrencyToken()
            .ValueGeneratedNever().HasDefaultValue(0);
    }
}
