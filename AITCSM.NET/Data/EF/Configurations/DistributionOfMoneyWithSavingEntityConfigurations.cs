using AITCSM.NET.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AITCSM.NET.Data.EF.Configurations;

public class DistributionOfMoneyWithSavingEntityConfigurations : IEntityTypeConfiguration<DistributionOfMoneyWithSaving>
{
    public void Configure(EntityTypeBuilder<DistributionOfMoneyWithSaving> builder)
    {
        builder
            .ToTable("DistributionOfMoneyWithSaving")
            .HasKey(d => d.Id);

        builder
            .HasMany(item => item.Results)
            .WithOne()
            .HasForeignKey(item => item.DistributionOfMoneyWithSavingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}