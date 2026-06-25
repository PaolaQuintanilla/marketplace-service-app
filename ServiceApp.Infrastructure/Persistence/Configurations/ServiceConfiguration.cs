using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceApp.Domain.Entities;

namespace ServiceApp.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(120);
        builder.Property(s => s.Category).IsRequired().HasMaxLength(80);
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasIndex(s => s.Category);
    }
}
