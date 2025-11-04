using Mechanics.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
{
    public void Configure(EntityTypeBuilder<BudgetItem> builder)
    {
        builder.ToTable("BudgetItems");

        builder.Property(i => i.BudgetId).IsRequired();
        builder.Property(i => i.NameSnapshot).IsRequired().HasMaxLength(500);
        builder.Property(i => i.UnitPriceSnapshot).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.Subtotal).HasColumnType("decimal(18,2)");
    }
}
