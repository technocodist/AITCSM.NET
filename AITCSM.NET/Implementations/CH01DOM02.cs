using AITCSM.NET.Base;

namespace AITCSM.NET.Implementations;

public static class CH01DOM02
{
    public record DOMSavingInput(int Id, int NumberOfAgents, double InitialMoney, int NumberOfIterations, double Lambda) : Identifyable(Id);
    public record DOMSavingOutput(int Id, DOMSavingInput Input, double[] Agents) : Identifyable(Id);

    public static async Task DefaultSimulate()
    {
        DOMSavingInput[] domSavingInputs = [
            new DOMSavingInput(Id: 1,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 100_000, Lambda: 0.1),
            new DOMSavingInput(Id: 2,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 100_000, Lambda: 0.2),
            new DOMSavingInput(Id: 3,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 100_000, Lambda: 0.3),
            new DOMSavingInput(Id: 4,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 200_000, Lambda: 0.1),
            new DOMSavingInput(Id: 5,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 200_000, Lambda: 0.2),
            new DOMSavingInput(Id: 6,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 200_000, Lambda: 0.3),
            new DOMSavingInput(Id: 7,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 400_000, Lambda: 0.1),
            new DOMSavingInput(Id: 8,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 400_000, Lambda: 0.2),
            new DOMSavingInput(Id: 9,NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 400_000, Lambda: 0.3)
        ];

        IEnumerable<DOMSavingOutput> domSavingOutputs = await Common.BatchSimulate(domSavingInputs, Simulate);
        await Common.WriteToJson(domSavingOutputs);
    }

    public static DOMSavingOutput Simulate(DOMSavingInput input)
    {
        Common.Log($"DOMSavingInput-{input.GetHashCode()} processing started!");
        double[] agents = [.. Enumerable.Range(0, input.NumberOfAgents).Select(_ => input.InitialMoney)];

        for (int i = 0; i < input.NumberOfIterations; i++)
        {
            int randI = Random.Shared.Next(0, input.NumberOfAgents);
            int randJ = Random.Shared.Next(0, input.NumberOfAgents);

            if (randI == randJ)
            {
                continue;
            }

            double epsilon = Random.Shared.NextDouble();
            double deltaMoney = (1 - input.Lambda) * (epsilon * agents[randJ] - (1 - epsilon) * agents[randI]);

            agents[randI] += deltaMoney;
            agents[randJ] -= deltaMoney;

        }

        Common.Log($"DOMSavingInput-{input.GetHashCode()} processing finished!");
        return new(input.Id, input, agents);
    }
}