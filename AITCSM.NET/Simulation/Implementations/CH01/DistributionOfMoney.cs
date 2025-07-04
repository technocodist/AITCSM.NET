using AITCSM.NET.Simulation.Abstractions;
using AITCSM.NET.Simulation.Abstractions.Entity;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AITCSM.NET.Simulation.Implementations.CH01;

public record DOMInput(int Id, int NumberOfAgents, double InitialMoney, int NumberOfIterations) : Identifyable(Id);
public record DOMOutput(int Id, DOMInput Input, double[] Agents) : Identifyable(Id);

public class DistributionOfMoney : ISimulation<DOMInput, DOMOutput>, IPlotable<DOMOutput>
{
    public static DOMInput[] Inputs { get; } = [
        new DOMInput(Id: 1, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 100_000),
        new DOMInput(Id: 2, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 200_000),
        new DOMInput(Id: 3, NumberOfAgents: 100, InitialMoney: 1000.0D, NumberOfIterations: 400_000)
    ];

    public static readonly Lazy<DistributionOfMoney> Instance = new(() => new DistributionOfMoney());

    public async IAsyncEnumerable<PlottingResult> Plot(DOMOutput output, PlottingOptions options)
    {
        Debug.Assert(output.Input is not null, "DOMOutput.Input must not be null.");
        Debug.Assert(output.Agents is not null, "Agents array must not be null.");
        Debug.Assert(output.Agents.Length == output.Input.NumberOfAgents,
            $"Agents array length ({output.Agents.Length}) must match NumberOfAgents ({output.Input.NumberOfAgents}).");

        Common.Log($"Plotting {output.GetUniqueName()} started!");

        ScottPlot.Plot plt = new();
        plt.Add.Scatter(
            [.. Enumerable.Range(0, output.Input.NumberOfAgents).Select(x => (double)x)],
            output.Agents);

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

    public Task<DOMOutput> Simulate(DOMInput input, CancellationToken ct)
    {
        Debug.Assert(input is not null, "Input must not be null.");
        Debug.Assert(input.NumberOfAgents > 1, "NumberOfAgents must be greater than 1.");
        Debug.Assert(input.InitialMoney >= 0.0, "InitialMoney must not be negative.");
        Debug.Assert(input.NumberOfIterations > 0, "NumberOfIterations must be positive.");

        Common.Log($"{input.GetUniqueName()} processing started!");

        double[] agents = [.. Enumerable.Range(0, input.NumberOfAgents).Select(_ => input.InitialMoney)];
        Debug.Assert(agents.Length == input.NumberOfAgents, "Agents array was not properly initialized.");

        for (int i = 0; i < input.NumberOfIterations; i++)
        {
            int randI = Random.Shared.Next(0, input.NumberOfAgents);
            int randJ = Random.Shared.Next(0, input.NumberOfAgents);

            Debug.Assert(randI >= 0 && randI < input.NumberOfAgents, "randI is out of valid agent index range.");
            Debug.Assert(randJ >= 0 && randJ < input.NumberOfAgents, "randJ is out of valid agent index range.");

            if (randI == randJ)
            {
                continue;
            }

            double epsilon = Random.Shared.NextDouble();
            Debug.Assert(epsilon >= 0.0 && epsilon <= 1.0, "Epsilon must be between 0.0 and 1.0.");

            double totalMoney = agents[randI] + agents[randJ];

            Debug.Assert(totalMoney >= 0.0, "Total money between two agents must not be negative.");

            agents[randI] = epsilon * totalMoney;
            agents[randJ] = (1.0 - epsilon) * totalMoney;
        }

        Common.Log($"{input.GetUniqueName()} processing finished!");
        return Task.FromResult(new DOMOutput(input.Id, input, agents));
    }

    public static async Task DefaultSimulate()
    {
        Debug.Assert(Inputs is not null && Inputs.Length > 0, "domInputs must not be null or empty.");
        CancellationToken ct = new();

        IEnumerable<DOMOutput> domOutputs = await Common.BatchOperate(Inputs, input => Instance.Value.Simulate(input, ct));
        Debug.Assert(domOutputs is not null, "BatchSimulate returned null.");
        await Common.WriteToJson(domOutputs);
    }

    public static async Task DefaultPlot()
    {
        DOMOutput?[] outputs = Common.ReadToObject<DOMOutput>(typeof(DOMOutput).FullName!);
        Debug.Assert(outputs is not null, "ReadToObject returned null.");

        await Common.BatchOperate(
            outputs.Where(output => output is { }).Cast<DOMOutput>().ToArray(),
            output => Instance.Value.Plot(output, Common.PlottingOptions));
    }
}