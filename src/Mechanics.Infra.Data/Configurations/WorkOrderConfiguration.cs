using Mechanics.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.Property(entity => entity.Status).IsRequired();

        builder.Property(entity => entity.LastUpdate).IsRequired()
            .HasDefaultValueSql("SYSDATETIME()")
            .ValueGeneratedOnAdd();

        builder.Property(entity => entity.AccessKey).HasMaxLength(8).IsRequired();
        builder.HasIndex(entity => new { entity.CustomerId, entity.AccessKey }).IsUnique();

        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(entity => entity.Vehicle)
            .WithMany()
            .HasForeignKey(entity => entity.VehicleId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasMany(entity => entity.Products)
            .WithMany(entity => entity.WorkOrders)
            .UsingEntity("WorkOrderProducts");

        builder.Property(e => e.ReportedProblem).HasMaxLength(1000);
        builder.Property(e => e.Observations).HasMaxLength(2000);
        builder.Property(e => e.IsCancelled).HasDefaultValue(false);
        builder.Property(e => e.LastStatusChangeBy).HasColumnType("uniqueidentifier");
    }
}
