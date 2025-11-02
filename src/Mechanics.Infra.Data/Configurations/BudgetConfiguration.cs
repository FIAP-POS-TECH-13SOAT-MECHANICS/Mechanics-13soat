using Mechanics.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.WorkOrderId).IsRequired();
        builder.Property(b => b.CreationDate)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");
        builder.Property(b => b.ExpiresAt).HasColumnType("datetime2");
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.Total).HasColumnType("decimal(18,2)");
        builder.Property(b => b.ApprovedAt).HasColumnType("datetime2");
        builder.Property(b => b.ApprovedByCustomerDocument).HasMaxLength(20);

        builder.HasOne(b => b.WorkOrder)
            .WithMany()
            .HasForeignKey(b => b.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Budget)
            .HasForeignKey(i => i.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
