using AITCSM.NET.Data.EF;
using AITCSM.NET.Data.Entities;
using AITCSM.NET.Simulation.Abstractions;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AITCSM.NET.Simulation.Implementations.CH01;

public class DistributionOfMoneyWithSavingSimulation :
    ISimulation<DistributionOfMoneyWithSaving, DistributionOfMoneyWithSavingStepResult>,
    IPlotable<DistributionOfMoneyWithSavingStepResult>
{
    public static DistributionOfMoneyWithSaving[] Inputs { get; } = [
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 100_000, Lambda= 0.1, InitialRandomSeed = 59, ResultPerSteps = 10},
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 100_000, Lambda= 0.2, InitialRandomSeed = 59, ResultPerSteps = 10},
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 100_000, Lambda= 0.3, InitialRandomSeed = 59, ResultPerSteps = 10},
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 200_000, Lambda= 0.1, InitialRandomSeed = 59, ResultPerSteps = 10},
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 200_000, Lambda= 0.2, InitialRandomSeed = 59, ResultPerSteps = 10},
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 200_000, Lambda= 0.3, InitialRandomSeed = 59, ResultPerSteps = 10},
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 400_000, Lambda= 0.1, InitialRandomSeed = 59, ResultPerSteps = 10},
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 400_000, Lambda= 0.2, InitialRandomSeed = 59, ResultPerSteps = 10},
        new()  {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 400_000, Lambda= 0.3, InitialRandomSeed = 59, ResultPerSteps = 10}
    ];

    public static readonly Lazy<DistributionOfMoneyWithSavingSimulation> Instance = new(() => new DistributionOfMoneyWithSavingSimulation());

    public async IAsyncEnumerable<DistributionOfMoneyWithSavingStepResult> Simulate(
        DistributionOfMoneyWithSaving input,
        [EnumeratorCancellation] CancellationToken ct)
    {
        Debug.Assert(input is not null, "Input must not be null.");
        Debug.Assert(input.NumberOfAgents > 1, "NumberOfAgents must be greater than 1.");
        Debug.Assert(input.InitialMoney >= 0.0, "InitialMoney must not be negative.");
        Debug.Assert(input.NumberOfIterations > 0, "NumberOfIterations must be positive.");
        Debug.Assert(input.Lambda >= 0.0 && input.Lambda <= 1.0, "Lambda must be between 0.0 and 1.0.");

        Common.Log($"{input.GetUniqueName()} processing started!");

        double[] agents = [.. Enumerable.Range(0, input.NumberOfAgents).Select(_ => input.InitialMoney)];
        Debug.Assert(agents.Length == input.NumberOfAgents, "Agents array was not properly initialized.");

        Random random = new(input.InitialRandomSeed);

        for (int i = 0; i < input.NumberOfIterations; i++)
        {
            ct.ThrowIfCancellationRequested();

            int randI = random.Next(0, input.NumberOfAgents);
            int randJ = random.Next(0, input.NumberOfAgents);

            Debug.Assert(randI >= 0 && randI < input.NumberOfAgents, "randI out of bounds.");
            Debug.Assert(randJ >= 0 && randJ < input.NumberOfAgents, "randJ out of bounds.");

            if (randI == randJ)
            {
                continue;
            }

            double epsilon = random.NextDouble();
            Debug.Assert(epsilon >= 0.0 && epsilon <= 1.0, "Epsilon must be between 0.0 and 1.0.");

            double deltaMoney = (1 - input.Lambda) * (epsilon * agents[randJ] - (1 - epsilon) * agents[randI]);

            agents[randI] += deltaMoney;
            agents[randJ] -= deltaMoney;

            if (i % input.ResultPerSteps == 0)
            {
                yield return new DistributionOfMoneyWithSavingStepResult()
                {
                    DistributionOfMoneyWithSavingId = input.Id,
                    StepNumber = i,
                    MoneyDistribution = [.. agents]
                };
            }
        }

        Common.Log($"{input.GetUniqueName()} processing finished!");
        await Task.CompletedTask;
    }

    public async IAsyncEnumerable<PlottingResult> Plot(DistributionOfMoneyWithSavingStepResult output, PlottingOptions options)
    {
        Debug.Assert(output.MoneyDistribution is not null, "Output.Agents must not be null.");

        Common.Log($"Plotting {output.GetUniqueName()} started!");

        ScottPlot.Plot plt = new();
        plt.Add.Scatter(
            [.. Enumerable.Range(0, output.MoneyDistribution.Length).Select(x => (double)x)],
            output.MoneyDistribution);

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
        Debug.Assert(Inputs is not null && Inputs.Length > 0, "DistributionOfMoneyWithSaving must not be null or empty.");


        await Task.Run(async () =>
        {
            using IServiceScope scope = DI.ServiceProvider.CreateScope();
            AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
            await context.DistributionOfMoneyWithSaving.AddRangeAsync(Inputs);
            await context.SaveChangesAsync();
        });

        IAsyncEnumerable<DistributionOfMoneyWithSavingStepResult> outputTasks = Instance.Value.RunConcurrentSimulations(
            Inputs,
            degreeOfParallelism: Environment.ProcessorCount / 2);

        ConcurrentBag<DistributionOfMoneyWithSavingStepResult> resultsBag = [];

        await foreach (DistributionOfMoneyWithSavingStepResult output in outputTasks)
        {
            resultsBag.Add(output);

            if (resultsBag.Count >= Common.BatchSize)
            {
                List<DistributionOfMoneyWithSavingStepResult> currentBatch = [.. resultsBag];
                resultsBag.Clear();

                _ = Task.Run(async () =>
                {
                    using IServiceScope scope = DI.ServiceProvider.CreateScope();
                    AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
                    await context.DistributionOfMoneyWithSavingStepResults.AddRangeAsync(currentBatch);
                    await context.SaveChangesAsync();
                });

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Console.WriteLine($"Saved {currentBatch.Count} simulation results to DB.");
                });
            }
        }

        if (!resultsBag.IsEmpty)
        {
            List<DistributionOfMoneyWithSavingStepResult> finalBatch = [.. resultsBag];
            resultsBag.Clear();

            if (finalBatch.Count > 0)
            {
                await Task.Run(async () =>
                {
                    using IServiceScope scope = DI.ServiceProvider.CreateScope();
                    AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
                    await context.DistributionOfMoneyWithSavingStepResults.AddRangeAsync(finalBatch);
                    await context.SaveChangesAsync();
                });

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Console.WriteLine($"Saved remaining {finalBatch.Count} simulation results to DB.");
                });
            }
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Console.WriteLine("Simulation and all database saves complete!");
        });
    }
}