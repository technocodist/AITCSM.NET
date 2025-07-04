using AITCSM.NET.Simulation.Abstractions;
using AITCSM.NET.Simulation.Abstractions.Entity;
using System.Diagnostics;

namespace AITCSM.NET.Simulation.Implementations.CH01;

public record DOMSavingInput(int Id, int NumberOfAgents, double InitialMoney, int NumberOfIterations, double Lambda) : Identifyable(Id);
public record DOMSavingOutput(int Id, DOMSavingInput Input, double[] Agents) : Identifyable(Id);

public class DistributionOfMoneyWithSaving : ISimulation<DOMSavingInput, DOMSavingOutput>, IPlotable<DOMSavingOutput>
{
    public static DOMSavingInput[] Inputs { get; } = [
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

    public static readonly Lazy<DistributionOfMoneyWithSaving> Instance = new(() => new DistributionOfMoneyWithSaving());

    public Task<DOMSavingOutput> Simulate(DOMSavingInput input, CancellationToken ct)
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
        return Task.FromResult(new DOMSavingOutput(input.Id, input, agents));
    }

    public async IAsyncEnumerable<PlottingResult> Plot(DOMSavingOutput output, PlottingOptions options)
    {
        Debug.Assert(output.Input is not null, "Output.Input must not be null.");
        Debug.Assert(output.Agents is not null, "Output.Agents must not be null.");
        Debug.Assert(output.Agents.Length == output.Input.NumberOfAgents,
            $"Agents array length ({output.Agents.Length}) must match NumberOfAgents ({output.Input.NumberOfAgents}).");

        Common.Log($"Plotting {output.GetUniqueName()} started!");

        ScottPlot.Plot plt = new();
        plt.Add.Scatter([
            .. Enumerable.Range(0, output.Input.NumberOfAgents).Select(x => (double)x)
        ], output.Agents);

        yield return new PlottingResult(
            Name: output.GetUniqueName(),
            ImageBytes: plt.GetImage(options.Width, options.Height).GetImageBytes(),
            Format: options.Format,
            Width: options.Width,
            Height: options.Height
        );

        Common.Log($"Plotting {output.GetUniqueName()} finished!");
        await Task.CompletedTask;
    }

    public static async Task DefaultSimulate()
    {
        Debug.Assert(Inputs is not null && Inputs.Length > 0, "Input array must not be null or empty.");
        CancellationToken ct = new();
        IEnumerable<DOMSavingOutput> domSavingOutputs = await Common.BatchOperate(Inputs, input => Instance.Value.Simulate(input, ct));
        Debug.Assert(domSavingOutputs is not null, "BatchSimulate returned null.");

        await Common.WriteToJson(domSavingOutputs);
    }

    public static async Task DefaultPlot()
    {
        DOMSavingOutput?[] outputs = Common.ReadToObject<DOMSavingOutput>(typeof(DOMSavingOutput).FullName!);
        Debug.Assert(outputs is not null, "ReadToObject returned null.");

        await Common.BatchOperate(
            outputs.Where(output => output is { }).Cast<DOMSavingOutput>().ToArray(),
            output => Instance.Value.Plot(output, Common.PlottingOptions));
    }
}