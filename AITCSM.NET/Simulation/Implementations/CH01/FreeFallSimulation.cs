using AITCSM.NET.Data.EF;
using AITCSM.NET.Data.Entities;
using AITCSM.NET.Simulation.Abstractions;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AITCSM.NET.Simulation.Implementations.CH01;

public class FreeFallSimulation : ISimulation<FreeFall, FreeFallStepResult>, IPlotable<FreeFallStepResult>
{
    public static readonly FreeFall[] Inputs = [
        new() {TimeStep= 0.01, StepCount =  1000, InitialVelocity=  10, InitialHeight = 0, Mass= 1,        Gravity= 9.81,  ResultPerSteps = 10},
        new() {TimeStep= 0.1, StepCount =  100, InitialVelocity=  0, InitialHeight = 100, Mass= 2,         Gravity= 9.81,  ResultPerSteps = 1},
        new() {TimeStep= 1.0, StepCount =  10, InitialVelocity=  -5, InitialHeight = 50, Mass= 5,          Gravity= 9.81,  ResultPerSteps = 1},
        new() {TimeStep= 0.05, StepCount =  200, InitialVelocity=  20, InitialHeight = 10, Mass= 0.5,      Gravity= 1.62,  ResultPerSteps = 10},
        new() {TimeStep= 0.01, StepCount =  500, InitialVelocity=  15, InitialHeight = 5, Mass= 10,        Gravity= 3.71,  ResultPerSteps = 10},
        new() {TimeStep= 0.001, StepCount =  10000, InitialVelocity=  50, InitialHeight = 100, Mass= 100,  Gravity= 9.81,  ResultPerSteps = 10},
        new() {TimeStep= 0.1, StepCount =  0, InitialVelocity=  0, InitialHeight = 0, Mass= 1,             Gravity= 9.81,  ResultPerSteps = 1},
        new() {TimeStep= 0.1, StepCount =  100, InitialVelocity=  0, InitialHeight = 0, Mass= 0.0001,      Gravity= 9.81,  ResultPerSteps = 10},
        new() {TimeStep= 0.1, StepCount =  100, InitialVelocity=  1000, InitialHeight = 10000, Mass= 1000, Gravity= 9.81,  ResultPerSteps = 10},
        new() {TimeStep= 0.1, StepCount =  100, InitialVelocity=  0, InitialHeight = 0, Mass= 1,           Gravity= 0 ,    ResultPerSteps = 10},
    ];

    public static readonly Lazy<FreeFallSimulation> Instance = new(() => new FreeFallSimulation());

    public async IAsyncEnumerable<FreeFallStepResult> Simulate(
        FreeFall input,
        [EnumeratorCancellation] CancellationToken ct)
    {
        Debug.Assert(input is not null, "Input is null.");
        Debug.Assert(input.StepCount >= 0, "StepCount must be non-negative.");
        Debug.Assert(input.TimeStep > 0, "TimeStep must be positive.");
        Debug.Assert(input.Mass > 0, "Mass must be positive.");
        Debug.Assert(input.Gravity >= 0, "Gravity must be non-negative.");

        Common.Log($"{input.GetUniqueName()} processing started!");

        double[] TimeSteps = new double[input.StepCount];
        double[] Velocities = new double[input.StepCount];
        double[] Positions = new double[input.StepCount];

        double time = 0.0;
        double velocity = input.InitialVelocity;
        double position = input.InitialHeight;
        double gravityForce = -input.Mass * input.Gravity;
        double acceleration = gravityForce / input.Mass;

        for (int step = 0; step < input.StepCount; step++)
        {
            ct.ThrowIfCancellationRequested();

            velocity += acceleration * input.TimeStep;
            position += velocity * input.TimeStep;
            time += input.TimeStep;

            TimeSteps[step] = time;
            Velocities[step] = velocity;
            Positions[step] = position;

            if (step % input.ResultPerSteps == 0)
            {
                yield return new()
                {
                    FreeFallId = input.Id,
                    StepNumber = step + 1,
                    TimeSteps = [.. TimeSteps],
                    Positions = [.. Positions],
                    Velocities = [.. Velocities],
                };
            }
        }

        Common.Log($"{input.GetUniqueName()} processing finished!");
        await Task.CompletedTask;
    }

    public async IAsyncEnumerable<PlottingResult> Plot(FreeFallStepResult output, PlottingOptions options)
    {
        Debug.Assert(output is not null, "Output is null.");
        Debug.Assert(output.TimeSteps is not null, "TimeSteps array is null.");
        Debug.Assert(output.Velocities is not null, "Velocities array is null.");
        Debug.Assert(output.Positions is not null, "Positions array is null.");

        Debug.Assert(output.Positions.Length == output.Velocities.Length, $"Positions.Length != Velocities.Length");
        Debug.Assert(output.Positions.Length == output.TimeSteps.Length, $"Positions.Length != TimeSteps.Length");

        Common.Log($"Plotting {output.GetUniqueName()} started!");

        ScottPlot.Plot plt = new();

        plt.Add.Scatter(output.TimeSteps, output.Velocities);
        plt.Title("Time vs. Velocity Diagram");
        plt.XLabel("Time Steps");
        plt.YLabel("Velocities");

        yield return new PlottingResult(
            Name: $"{output.GetUniqueName()}-time-velocity",
            ImageBytes: plt.GetImage(options.Width, options.Height).GetImageBytes(),
            Format: options.Format,
            Width: options.Width,
            Height: options.Height
        );

        plt.Clear();
        plt.Add.Scatter(output.TimeSteps, output.Positions);
        plt.Title("Time vs. Position Diagram");
        plt.XLabel("Time Steps");
        plt.YLabel("Positions");

        yield return new PlottingResult(
            Name: $"{output.GetUniqueName()}-time-position",
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
        Debug.Assert(Inputs is not null && Inputs.Length > 0, "domInputs must not be null or empty.");

        await Task.Run(async () =>
        {
            using IServiceScope scope = DI.ServiceProvider.CreateScope();
            AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
            await context.FreeFall.AddRangeAsync(Inputs);
            await context.SaveChangesAsync();
        });

        IAsyncEnumerable<FreeFallStepResult> outputTasks = Instance.Value.RunConcurrentSimulations(
            Inputs,
            degreeOfParallelism: Environment.ProcessorCount / 2);

        ConcurrentBag<FreeFallStepResult> resultsBag = [];

        await foreach (FreeFallStepResult output in outputTasks)
        {
            resultsBag.Add(output);

            if (resultsBag.Count >= Common.BatchSize)
            {
                List<FreeFallStepResult> currentBatch = new();
                while (resultsBag.TryTake(out FreeFallStepResult? item))
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
                        await context.FreeFallStepResults.AddRangeAsync(currentBatch);
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
            List<FreeFallStepResult> finalBatch = new();
            while (resultsBag.TryTake(out FreeFallStepResult? item))
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
                    await context.FreeFallStepResults.AddRangeAsync(finalBatch);
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