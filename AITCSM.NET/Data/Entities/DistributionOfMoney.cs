using AITCSM.NET.Data.Entities.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace AITCSM.NET.Data
{
    public class DistributionOfMoney : EntityBase
    {
        public int NumberOfAgents { get; set; }
        public double InitialMoney { get; set; }
        public int NumberOfIterations { get; set; }
        public int InitialRandomSeed { get; set; }
        public int ResultPerSteps { get; set; }
        public virtual List<DistributionOfMoneyStepResult> Results { get; protected set; } = [];
    }

    public class DistributionOfMoneyStepResult : EntityBase
    {
        public int DistributionOfMoneyId { get; set; }
        public int StepNumber { get; set; }

        public string MoneyDistributionData
        {
            get => string.Join(';', MoneyDistribution);
            set => MoneyDistribution = (value == null) ? [] : Array.ConvertAll(value.Split(';', StringSplitOptions.RemoveEmptyEntries), double.Parse);
        }

        [NotMapped]
        public double[] MoneyDistribution { get; set; } = [];
    }
}