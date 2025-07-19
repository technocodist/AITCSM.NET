using AITCSM.NET.Data.Entities.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AITCSM.NET.Data.Entities;

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

    [JsonIgnore]
    public byte[] MoneyDistributionBytes
    {
        get
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(MoneyDistribution.Length);
                foreach (double d in MoneyDistribution)
                {
                    writer.Write(d);
                }
            }
            return stream.ToArray();
        }
        set
        {
            if (value == null)
            {
                MoneyDistribution = [];
                return;
            }
            using MemoryStream stream = new(value);
            using BinaryReader reader = new(stream);
            int length = reader.ReadInt32();
            MoneyDistribution = new double[length];
            for (int i = 0; i < length; i++)
            {
                MoneyDistribution[i] = reader.ReadDouble();
            }
        }
    }

    [NotMapped]
    public double[] MoneyDistribution { get; set; } = [];
}