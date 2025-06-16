using System.Diagnostics;
using AITCSM.NET.Base;

namespace AITCSM.NET.Implementations;

public static class CH01DOM02
{
    public record DOMSavingInput(int Id, int NumberOfAgents, double InitialMoney, int NumberOfIterations, double Lambda) : Identifyable(Id);
    public record DOMSavingOutput(int Id, DOMSavingInput Input, double[] Agents) : Identifyable(Id);

    public static async Task DefaultSimulate()
    {
        DOMSavingInput[] domSavingInputs = [
            new DOMSavingInput(Id: 1, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 100_000, Lambda: 0.1),
            new DOMSavingInput(Id: 2, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 100_000, Lambda: 0.2),
            new DOMSavingInput(Id: 3, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 100_000, Lambda: 0.3),
            new DOMSavingInput(Id: 4, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 200_000, Lambda: 0.1),
            new DOMSavingInput(Id: 5, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 200_000, Lambda: 0.2),
            new DOMSavingInput(Id: 6, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 200_000, Lambda: 0.3),
            new DOMSavingInput(Id: 7, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 400_000, Lambda: 0.1),
            new DOMSavingInput(Id: 8, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 400_000, Lambda: 0.2),
            new DOMSavingInput(Id: 9, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 400_000, Lambda: 0.3)
        ];

        Debug.Assert(domSavingInputs is not null && domSavingInputs.Length > 0, "Input array must not be null or empty.");

        IEnumerable<DOMSavingOutput> domSavingOutputs = await Common.BatchOperate(domSavingInputs, Simulate);
        Debug.Assert(domSavingOutputs is not null, "BatchSimulate returned null.");

        await Common.WriteToJson(domSavingOutputs);
    }

    public static async Task DefaultPlot()
    {
        DOMSavingOutput?[] outputs = Common.ReadToObject<DOMSavingOutput>(typeof(DOMSavingOutput).FullName!);
        Debug.Assert(outputs is not null, "ReadToObject returned null.");

        static void Plotter(DOMSavingOutput output)
        {
            Debug.Assert(output.Input is not null, "Output.Input must not be null.");
            Debug.Assert(output.Agents is not null, "Output.Agents must not be null.");
            Debug.Assert(output.Agents.Length == output.Input.NumberOfAgents,
                $"Agents array length ({output.Agents.Length}) must match NumberOfAgents ({output.Input.NumberOfAgents}).");

            Common.Log($"Plotting {output.GetUniqueName()} started!");

            ScottPlot.Plot plt = new();
            plt.Add.Scatter([.. Enumerable.Range(0, output.Input.NumberOfAgents).Select(x => (double)x)], output.Agents);

            plt.SaveSvg(
                Path.Combine(Common.OutputDir, $"{output.GetUniqueName()}.svg"),
                width: 1920,
                height: 1080);

            Common.Log($"Plotting {output.GetUniqueName()} finished!");
        }

        await Common.BatchOperate(
            outputs.Where(output => output is { }).Cast<DOMSavingOutput>().ToArray(),
            Plotter);
    }

    public static DOMSavingOutput Simulate(DOMSavingInput input)
    {
        Debug.Assert(input is not null, "Input must not be null.");
        Debug.Assert(input.NumberOfAgents > 1, "NumberOfAgents must be greater than 1.");
        Debug.Assert(input.InitialMoney >= 0.0, "InitialMoney must not be negative.");
        Debug.Assert(input.NumberOfIterations > 0, "NumberOfIterations must be positive.");
        Debug.Assert(input.Lambda >= 0.0 && input.Lambda <= 1.0, "Lambda must be between 0.0 and 1.0.");

        Common.Log($"{input.GetUniqueName()} processing started!");

        double[] agents = [.. Enumerable.Range(0, input.NumberOfAgents).Select(_ => input.InitialMoney)];
        Debug.Assert(agents.Length == input.NumberOfAgents, "Agents array was not properly initialized.");

        for (int i = 0; i < input.NumberOfIterations; i++)
        {
            int randI = Random.Shared.Next(0, input.NumberOfAgents);
            int randJ = Random.Shared.Next(0, input.NumberOfAgents);

            Debug.Assert(randI >= 0 && randI < input.NumberOfAgents, "randI out of bounds.");
            Debug.Assert(randJ >= 0 && randJ < input.NumberOfAgents, "randJ out of bounds.");

            if (randI == randJ)
            {
                continue;
            }

            double epsilon = Random.Shared.NextDouble();
            Debug.Assert(epsilon >= 0.0 && epsilon <= 1.0, "Epsilon must be between 0.0 and 1.0.");

            double deltaMoney = (1 - input.Lambda) * (epsilon * agents[randJ] - (1 - epsilon) * agents[randI]);

            agents[randI] += deltaMoney;
            agents[randJ] -= deltaMoney;
        }

        Common.Log($"{input.GetUniqueName()} processing finished!");
        return new(input.Id, input, agents);
    }
}
