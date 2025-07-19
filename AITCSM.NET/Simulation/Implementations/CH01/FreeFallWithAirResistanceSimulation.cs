using AITCSM.NET.Data.EF;
using AITCSM.NET.Data.Entities;
using AITCSM.NET.Simulation.Abstractions;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AITCSM.NET.Simulation.Implementations.CH01;

public class FreeFallWithAirResistanceSimulation : ISimulation<FreeFallWithAirResistance, FreeFallWithAirResistanceStepResult>, IPlotable<FreeFallWithAirResistanceStepResult>
{
    public static FreeFallWithAirResistance[] Inputs { get; } = [
        new () {TimeStep=  00.01, VelocityEffectivePower= 1, StepCount= 100000, InitialVelocity= 10, InitialHeight= 0, Mass= 1, Gravity = 9.81,DragCoeffecient =  0.5, ResultPerSteps = 10},
        new () {TimeStep=  000.1, VelocityEffectivePower= 2, StepCount= 100000, InitialVelocity= 0, InitialHeight= 100, Mass= 2, Gravity = 9.81,DragCoeffecient =  0.9, ResultPerSteps = 10},
        new () {TimeStep=  001.0, VelocityEffectivePower= 1, StepCount= 100000, InitialVelocity= -5, InitialHeight= 50, Mass= 5, Gravity = 9.81,DragCoeffecient =  1, ResultPerSteps = 10},
        new () {TimeStep=  00.05, VelocityEffectivePower= 2, StepCount= 100000, InitialVelocity= 20, InitialHeight= 10, Mass= 0.5, Gravity = 1.62, DragCoeffecient =  4, ResultPerSteps = 10},
        new () {TimeStep=  00.01, VelocityEffectivePower= 2, StepCount= 100000, InitialVelocity= 15, InitialHeight= 5, Mass= 10, Gravity = 3.71,DragCoeffecient =  0.2, ResultPerSteps = 10},
        new () {TimeStep=  0.001, VelocityEffectivePower= 2, StepCount= 100000, InitialVelocity= 50, InitialHeight= 100, Mass= 100, Gravity = 9.81,DragCoeffecient =  0.7, ResultPerSteps = 10},
        new () {TimeStep=  000.1, VelocityEffectivePower= 1, StepCount= 100000, InitialVelocity= 0, InitialHeight= 0, Mass= 1, Gravity = 9.81,DragCoeffecient =  0.9, ResultPerSteps = 10},
        new () {TimeStep=  000.1, VelocityEffectivePower= 2, StepCount= 100000, InitialVelocity= 0, InitialHeight= 0, Mass= 0.0001, Gravity = 9.81, DragCoeffecient =  0.1, ResultPerSteps = 10},
        new () {TimeStep=  000.1, VelocityEffectivePower= 1, StepCount= 100000, InitialVelocity= 1000, InitialHeight= 10000, Mass= 1000, Gravity = 9.81,DragCoeffecient =  0.01, ResultPerSteps = 10},
        new () {TimeStep=  000.1, VelocityEffectivePower= 2, StepCount= 100000, InitialVelocity= 0, InitialHeight= 0, Mass= 1, Gravity = 0,DragCoeffecient =  0.25, ResultPerSteps = 10},
    ];

    public static readonly Lazy<FreeFallWithAirResistanceSimulation> Instance = new(() => new FreeFallWithAirResistanceSimulation());

    public async IAsyncEnumerable<FreeFallWithAirResistanceStepResult> Simulate(
        FreeFallWithAirResistance input,
        [EnumeratorCancellation] CancellationToken ct)
    {
        Debug.Assert(input is not null, "Input is null.");
        Debug.Assert(input.StepCount >= 0, "StepCount must be non-negative.");
        Debug.Assert(input.TimeStep > 0, "TimeStep must be positive.");
        Debug.Assert(input.Mass > 0, "Mass must be positive.");
        Debug.Assert(input.Gravity >= 0, "Gravity must be non-negative.");

        Common.Log($"{input.GetUniqueName()} processing started!");

        double[] timeSteps = new double[input.StepCount];
        double[] velocities = new double[input.StepCount];
        double[] positions = new double[input.StepCount];
        double[] dragForces = new double[input.StepCount];
        double[] netForces = new double[input.StepCount];

        double time = 0.0;
        double velocity = input.InitialVelocity;
        double position = input.InitialHeight;
        double gravityForce = -input.Mass * input.Gravity;

        for (int step = 0; step < input.StepCount; step++)
        {
            ct.ThrowIfCancellationRequested();

            double dragForce = 0.5D * input.DragCoeffecient * Math.Pow(velocity, input.VelocityEffectivePower);
            double netForce = (velocity > 0 ? -1 : 1) * dragForce + gravityForce;
            double acceleration = netForce / input.Mass;

            velocity += acceleration * input.TimeStep;
            position += velocity * input.TimeStep;
            time += input.TimeStep;

            timeSteps[step] = time;
            velocities[step] = velocity;
            positions[step] = position;
            dragForces[step] = dragForce;
            netForces[step] = netForce;

            if (step % input.ResultPerSteps == 0)
            {
                yield return new()
                {
                    FreeFallWithAirResistanceId = input.Id,
                    StepNumber = step + 1,
                    TimeSteps = timeSteps,
                    Velocities = velocities,
                    Positions = positions,
                    DragForces = dragForces,
                    NetForces = netForces
                };
            }
        }

        Common.Log($"{input.GetUniqueName()} processing finished!");

        await Task.CompletedTask;
    }

    public async IAsyncEnumerable<PlottingResult> Plot(FreeFallWithAirResistanceStepResult output, PlottingOptions options)
    {
        Debug.Assert(output is not null, "Output is null.");
        Debug.Assert(output.TimeSteps is not null, "TimeSteps array is null.");
        Debug.Assert(output.Velocities is not null, "Velocities array is null.");
        Debug.Assert(output.Positions is not null, "Positions array is null.");

        int n = output.TimeSteps.Length;
        Debug.Assert(output.TimeSteps.Length == n, $"TimeSteps.Length != StepCount ({n})");
        Debug.Assert(output.Velocities.Length == n, $"Velocities.Length != StepCount ({n})");
        Debug.Assert(output.Positions.Length == n, $"Positions.Length != StepCount ({n})");

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

        plt.Clear();
        plt.Add.Scatter(output.TimeSteps, output.NetForces);
        plt.Title("Time vs. Net Forces Diagram");
        plt.XLabel("Time Steps");
        plt.YLabel("Net Forces");

        yield return new PlottingResult(
            Name: $"{output.GetUniqueName()}-time-netforce",
            ImageBytes: plt.GetImage(options.Width, options.Height).GetImageBytes(),
            Format: options.Format,
            Width: options.Width,
            Height: options.Height
        );

        plt.Clear();
        plt.Add.Scatter(output.TimeSteps, output.DragForces);
        plt.Title("Time vs. Drag Forces Diagram");
        plt.XLabel("Time Steps");
        plt.YLabel("Drag Forces");

        yield return new PlottingResult(
            Name: $"{output.GetUniqueName()}-time-dragforce",
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
            await context.FreeFallWithAirResistance.AddRangeAsync(Inputs);
            await context.SaveChangesAsync();
        });

        IAsyncEnumerable<FreeFallWithAirResistanceStepResult> outputTasks = Instance.Value.RunConcurrentSimulations(
            Inputs,
            degreeOfParallelism: Environment.ProcessorCount / 2);

        ConcurrentBag<FreeFallWithAirResistanceStepResult> resultsBag = [];

        await foreach (FreeFallWithAirResistanceStepResult output in outputTasks)
        {
            resultsBag.Add(output);

            if (resultsBag.Count >= Common.BatchSize)
            {
                List<FreeFallWithAirResistanceStepResult> currentBatch = [.. resultsBag];
                resultsBag.Clear();

                _ = Task.Run(async () =>
                {
                    using IServiceScope scope = DI.ServiceProvider.CreateScope();
                    AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
                    await context.FreeFallWithAirResistanceStepResult.AddRangeAsync(currentBatch);
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
            List<FreeFallWithAirResistanceStepResult> finalBatch = [.. resultsBag];
            resultsBag.Clear();

            await Task.Run(async () =>
            {
                using IServiceScope scope = DI.ServiceProvider.CreateScope();
                AITCSMContext context = scope.ServiceProvider.GetRequiredService<AITCSMContext>();
                await context.FreeFallWithAirResistanceStepResult.AddRangeAsync(finalBatch);
                await context.SaveChangesAsync();
            });

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Console.WriteLine($"Saved remaining {finalBatch.Count} simulation results to DB.");
            });
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Console.WriteLine("Simulation and all database saves complete!");
        });
    }
}