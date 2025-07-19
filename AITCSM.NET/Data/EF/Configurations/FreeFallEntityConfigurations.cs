using AITCSM.NET.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AITCSM.NET.Data.EF.Configurations;

public class FreeFallEntityConfigurations : IEntityTypeConfiguration<FreeFall>
{
    public void Configure(EntityTypeBuilder<FreeFall> builder)
    {
        builder
            .ToTable("FreeFall")
            .HasKey(d => d.Id);

        builder
            .HasMany(item => item.Results)
            .WithOne()
            .HasForeignKey(item => item.FreeFallId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}