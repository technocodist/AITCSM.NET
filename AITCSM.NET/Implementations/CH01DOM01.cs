using AITCSM.NET.Base;

namespace AITCSM.NET.Implementations;

public static class CH01DOM01
{
    public record DOMInput(int Id, int NumberOfAgents, double InitialMoney, int NumberOfIterations) : Identifyable(Id);
    public record DOMOutput(int Id, DOMInput Input, double[] Agents) : Identifyable(Id);

    private static DOMInput[] domInputs { get; } = [
        new DOMInput(Id: 1, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 100_000),
        new DOMInput(Id: 2, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 200_000),
        new DOMInput(Id: 3, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 400_000)
    ];

    public static async Task DefaultSimulate()
    {
        IEnumerable<DOMOutput> domOutputs = await Common.BatchSimulate(domInputs, Simulate);
        await Common.WriteToJson(domOutputs);
    }

    public static void DefaultPlot()
    {
        DOMOutput?[] domOutputs = Common.ReadToObject<DOMOutput>(typeof(DOMOutput).FullName!);

        foreach (DOMOutput? output in domOutputs)
        {
            if (output == null)
            {
                continue;
            }
            
            Common.Log($"Plotting DOMOutput-{output.GetUniqueName()} processing started!");

            ScottPlot.Plot plt = new();
            plt.Add.Scatter([.. Enumerable.Range(0, output.Input.NumberOfAgents).Select(x => (double)x)], output.Agents);

            plt.SaveSvg(
                Path.Combine(Common.OutputDir, $"{output.GetUniqueName()}.svg"),
                width: 1920,
                height: 1080);
        }
    }

    public static DOMOutput Simulate(DOMInput input)
    {
        Common.Log($"DOMInput-{input.GetHashCode()} processing started!");
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
            double totalMoney = agents[randI] + agents[randJ];

            agents[randI] = epsilon * totalMoney;
            agents[randJ] = (1.0 - epsilon) * totalMoney;
        }

        Common.Log($"DOMInput-{input.GetHashCode()} processing finished!");
        return new DOMOutput(input.Id, input, agents);
    }
}