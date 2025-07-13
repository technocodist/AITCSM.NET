using AITCSM.NET.Data.EF;
using AITCSM.NET.Data.Entities;
using AITCSM.NET.Simulation.Abstractions;
using Avalonia.Animation.Easings;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AITCSM.NET.Simulation.Implementations.CH01;

public class DistributionOfMoneySimulation :
                    ISimulation<DistributionOfMoney, DistributionOfMoneyStepResult>,
                    IPlotable<DistributionOfMoneyStepResult>
{
    public static DistributionOfMoney[] Inputs { get; } = [
        new() {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 100_000, InitialRandomSeed = 59, ResultPerSteps = 10},
        new() {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 200_000, InitialRandomSeed = 59, ResultPerSteps = 10},
        new() {NumberOfAgents= 100, InitialMoney= 1000.0D, NumberOfIterations= 400_000, InitialRandomSeed = 59, ResultPerSteps = 10}
    ];

    public static readonly Lazy<DistributionOfMoneySimulation> Instance = new(() => new DistributionOfMoneySimulation());

    public async IAsyncEnumerable<PlottingResult> Plot(DistributionOfMoneyStepResult output, PlottingOptions options)
    {
        Debug.Assert(output.MoneyDistribution is not null, "Agents array must not be null.");

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

    public async IAsyncEnumerable<DistributionOfMoneyStepResult> Simulate(
        DistributionOfMoney input,
        [EnumeratorCancellation] CancellationToken ct)
    {
        Debug.Assert(input is not null, "Input must not be null.");
        Debug.Assert(input.NumberOfAgents > 1, "NumberOfAgents must be greater than 1.");
        Debug.Assert(input.InitialMoney >= 0.0, "InitialMoney must not be negative.");
        Debug.Assert(input.NumberOfIterations > 0, "NumberOfIterations must be positive.");

        Common.Log($"{input.GetUniqueName()} processing started!");

        double[] agentsMoneyDistribution = [.. Enumerable.Range(0, input.NumberOfAgents).Select(_ => input.InitialMoney)];
        Debug.Assert(agentsMoneyDistribution.Length == input.NumberOfAgents, "Agents array was not properly initialized.");

        Random random = new(input.InitialRandomSeed);

        for (int i = 0; i < input.NumberOfIterations; i++)
        {
            ct.ThrowIfCancellationRequested();

            int randI = random.Next(0, input.NumberOfAgents);
            int randJ = random.Next(0, input.NumberOfAgents);

            Debug.Assert(randI >= 0 && randI < input.NumberOfAgents, "randI is out of valid agent index range.");
            Debug.Assert(randJ >= 0 && randJ < input.NumberOfAgents, "randJ is out of valid agent index range.");

            if (randI == randJ)
            {
                continue;
            }

            double epsilon = random.NextDouble();
            Debug.Assert(epsilon >= 0.0 && epsilon <= 1.0, "Epsilon must be between 0.0 and 1.0.");

            double totalMoney = agentsMoneyDistribution[randI] + agentsMoneyDistribution[randJ];

            Debug.Assert(totalMoney >= 0.0, "Total money between two agents must not be negative.");

            agentsMoneyDistribution[randI] = epsilon * totalMoney;
            agentsMoneyDistribution[randJ] = (1.0 - epsilon) * totalMoney;

            if (i % input.ResultPerSteps == 0)
            {
                yield return new DistributionOfMoneyStepResult()
                {
                    DistributionOfMoneyId = input.Id,
                    StepNumber = i,
                    MoneyDistribution = [.. agentsMoneyDistribution]
                };
            }
        }

        Common.Log($"{input.GetUniqueName()} processing finished!");
        await Task.CompletedTask;
    }

    public static async Task DefaultSimulate()
    {
        Debug.Assert(Inputs is not null && Inputs.Length > 0, "domInputs must not be null or empty.");

        await Task.Run(async () =>
        {
            using IServiceScope scope = DI.ServiceProvider.CreateScope();
            AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
            await context.DistributionOfMoney.AddRangeAsync(Inputs);
            await context.SaveChangesAsync();
        });

        IAsyncEnumerable<DistributionOfMoneyStepResult> outputTasks = Instance.Value.RunConcurrentSimulations(
            Inputs,
            degreeOfParallelism: Environment.ProcessorCount);

        const int batchSize = 10_000;
        ConcurrentBag<DistributionOfMoneyStepResult> resultsBag = [];

        await foreach (DistributionOfMoneyStepResult output in outputTasks)
        {
            resultsBag.Add(output);

            if (resultsBag.Count >= batchSize)
            {
                List<DistributionOfMoneyStepResult> currentBatch = new();
                while (resultsBag.TryTake(out DistributionOfMoneyStepResult? item))
                {
                    if (item != null)
                    {
                        currentBatch.Add(item);
                    }
                }

                if (currentBatch.Count > 0)
                {
                    _ = Task.Run(async () =>
                    {
                        using IServiceScope scope = DI.ServiceProvider.CreateScope();
                        AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
                        await context.DistributionOfMoneyStepResults.AddRangeAsync(currentBatch);
                        await context.SaveChangesAsync();
                    });

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Console.WriteLine($"Saved {currentBatch.Count} simulation results to DB.");
                    });
                }
            }
        }

        if (!resultsBag.IsEmpty)
        {
            List<DistributionOfMoneyStepResult> finalBatch = new();
            while (resultsBag.TryTake(out DistributionOfMoneyStepResult? item))
            {
                if (item != null)
                {
                    finalBatch.Add(item);
                }
            }

            if (finalBatch.Count > 0)
            {
                await Task.Run(async () =>
                {
                    using IServiceScope scope = DI.ServiceProvider.CreateScope();
                    AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
                    await context.DistributionOfMoneyStepResults.AddRangeAsync(finalBatch);
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

    public static async Task DefaultPlot()
    {
        DistributionOfMoneyStepResult?[] outputs = Common.ReadToObject<DistributionOfMoneyStepResult>(typeof(DistributionOfMoneyStepResult).FullName!);
        Debug.Assert(outputs is not null, "ReadToObject returned null.");

        await Common.BatchOperate(
            outputs.Where(output => output is { }).Cast<DistributionOfMoneyStepResult>().ToArray(),
            output => Instance.Value.Plot(output, Common.PlottingOptions));
    }
}