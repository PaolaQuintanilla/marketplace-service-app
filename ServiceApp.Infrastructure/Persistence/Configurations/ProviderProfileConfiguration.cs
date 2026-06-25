using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceApp.Domain.Entities;

namespace ServiceApp.Infrastructure.Persistence.Configurations;

public class ProviderProfileConfiguration : IEntityTypeConfiguration<ProviderProfile>
{
    public void Configure(EntityTypeBuilder<ProviderProfile> builder)
    {
        builder.ToTable("Providers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Description).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.HourlyRate).HasPrecision(18, 2);

        // The User <-> ProviderProfile relationship is configured in UserConfiguration.
        builder.HasOne(p => p.Service)
            .WithMany(s => s.Providers)
            .HasForeignKey(p => p.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
