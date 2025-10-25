using Mechanics.Domain.ServicesCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class ServiceCatalogConfiguration : IEntityTypeConfiguration<ServiceCatalog>
{
    public void Configure(EntityTypeBuilder<ServiceCatalog> builder)
    {
        // Nome do serviço
        builder.Property(entity => entity.Name)
               .HasMaxLength(100)
               .IsRequired();

        // Descrição do serviço
        builder.Property(entity => entity.Description)
               .HasMaxLength(255);

        // Preço com precisão adequada
        builder.Property(entity => entity.BasePrice)
               .HasPrecision(10, 2); // até 99999999.99

        // Tempo médio de execução
        builder.Property(entity => entity.AverageTime)
               .IsRequired();

        // Status como enum
        builder.Property(entity => entity.Status)
               .HasConversion<int>()
               .IsRequired();
    }
}
