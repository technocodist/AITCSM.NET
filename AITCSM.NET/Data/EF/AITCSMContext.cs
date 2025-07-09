using Microsoft.EntityFrameworkCore;

namespace AITCSM.NET.Data.EF
{
    public class AITCSMContext(DbContextOptions<AITCSMContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AITCSMContext).Assembly);
        }

        public virtual DbSet<DistributionOfMoney> DistributionOfMoney { get; set; }
        public virtual DbSet<DistributionOfMoneyStepResult> DistributionOfMoneyStepResults { get; set; }
    }
}