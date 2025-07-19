using AITCSM.NET.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AITCSM.NET.Data.EF.Configurations;

public class FreeFallWithAirResistanceEntityConfigurations : IEntityTypeConfiguration<FreeFallWithAirResistance>
{
    public void Configure(EntityTypeBuilder<FreeFallWithAirResistance> builder)
    {
        builder
            .ToTable("FreeFallWithAirResistance")
            .HasKey(d => d.Id);

        builder
            .HasMany(item => item.Results)
            .WithOne()
            .HasForeignKey(item => item.FreeFallWithAirResistanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}