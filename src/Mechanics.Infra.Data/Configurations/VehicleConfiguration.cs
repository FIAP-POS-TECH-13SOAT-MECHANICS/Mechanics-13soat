using Mechanics.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.Property(v => v.Manufacturer).HasMaxLength(40).IsRequired();
        builder.Property(v => v.Model).HasMaxLength(60).IsRequired();
        builder.Property(v => v.Year).HasMaxLength(4).IsRequired();
        builder.Property(v => v.Color).HasConversion<string>().HasMaxLength(15).IsRequired();

        builder.OwnsOne(entity => entity.LicensePlate, plate =>
        {
            plate.Property(p => p.Number).HasColumnName(nameof(Vehicle.LicensePlate)).HasMaxLength(7);
            plate.HasIndex(p => p.Number).IsUnique();
        });
        builder.Navigation(entity => entity.LicensePlate).IsRequired();

        builder.Property(v => v.Chassis).IsRequired().HasMaxLength(17);
        builder.HasIndex(p => p.Chassis).IsUnique();

        builder.HasOne(entity => entity.Owner)
            .WithMany(entity => entity.Vehicles)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}
