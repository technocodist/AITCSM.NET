using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AITCSM.NET.Data.EF.Configurations;

public class DistributionOfMoneyEntityConfigurations : IEntityTypeConfiguration<DistributionOfMoney>
{
    public void Configure(EntityTypeBuilder<DistributionOfMoney> builder)
    {
        builder
            .ToTable("DistributionOfMoney")
            .HasKey(d => d.Id);

        builder
            .HasMany(item => item.Results)
            .WithOne()
            .HasForeignKey(item => item.DistributionOfMoneyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}