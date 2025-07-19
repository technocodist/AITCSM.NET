using AITCSM.NET.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AITCSM.NET.Data.EF;

public class AITCSMContext(DbContextOptions<AITCSMContext> options) :
    DbContext(options),
    IDesignTimeDbContextFactory<AITCSMContext>
{
    public AITCSMContext() : this(new())
    {

    }

    public AITCSMContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AITCSMContext> optionsBuilder = new();
        optionsBuilder.UseSqlite("Data Source=c://AITCSM/AITCSM.db;");

        return new AITCSMContext(optionsBuilder.Options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AITCSMContext).Assembly);
    }

    public virtual DbSet<DistributionOfMoney> DistributionOfMoney { get; set; }
    public virtual DbSet<DistributionOfMoneyStepResult> DistributionOfMoneyStepResults { get; set; }

    public virtual DbSet<DistributionOfMoneyWithSaving> DistributionOfMoneyWithSaving { get; set; }
    public virtual DbSet<DistributionOfMoneyWithSavingStepResult> DistributionOfMoneyWithSavingStepResults { get; set; }

    public virtual DbSet<FreeFall> FreeFall { get; set; }
    public virtual DbSet<FreeFallStepResult> FreeFallStepResults { get; set; }

    public virtual DbSet<FreeFallWithAirResistance> FreeFallWithAirResistance { get; set; }
    public virtual DbSet<FreeFallWithAirResistanceStepResult> FreeFallWithAirResistanceStepResult { get; set; }
}