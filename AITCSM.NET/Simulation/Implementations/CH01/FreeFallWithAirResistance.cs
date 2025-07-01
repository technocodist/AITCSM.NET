using AITCSM.NET.Simulation.Abstractions;
using AITCSM.NET.Simulation.Abstractions.Entity;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AITCSM.NET.Simulation.Implementations.CH01;

public record FFWARInput(
    int Id,
    double TimeStep,
    int StepCount,
    double VelocityEffectivePower,
    double InitialVelocity,
    double InitialHeight,
    double Mass,
    double Gravity,
    double DragCoeffecient) : Identifyable(Id);

public record FFWAROutput(
    int Id,
    FFWARInput Input,
    double[] TimeSteps,
    double[] Velocities,
    double[] Positions,
    double[] DragForces,
    double[] NetForces) : Identifyable(Id);

public class FreeFallWithAirResistance : ISimulation<FFWARInput, FFWAROutput>, IPlotable<FFWAROutput>
{
    public static FFWARInput[] Inputs { get; } = [
        new FFWARInput(Id: 1, TimeStep: 00.01, VelocityEffectivePower: 1,  StepCount: 100000,     InitialVelocity: 10, InitialHeight: 0, Mass: 1, Gravity: 9.81,  DragCoeffecient: 0.5),
        new FFWARInput(Id: 2, TimeStep: 000.1, VelocityEffectivePower: 2,  StepCount: 100000,  InitialVelocity: 0, InitialHeight: 100, Mass: 2, Gravity: 9.81,     DragCoeffecient: 0.9),
        new FFWARInput(Id: 3, TimeStep: 001.0, VelocityEffectivePower: 1,  StepCount: 100000,   InitialVelocity: -5, InitialHeight: 50, Mass: 5, Gravity: 9.81,     DragCoeffecient: 1),
        new FFWARInput(Id: 4, TimeStep: 00.05, VelocityEffectivePower: 2,  StepCount: 100000,  InitialVelocity: 20, InitialHeight: 10, Mass: 0.5, Gravity: 1.62,   DragCoeffecient: 4),
        new FFWARInput(Id: 5, TimeStep: 00.01, VelocityEffectivePower: 2,  StepCount: 100000,  InitialVelocity: 15, InitialHeight: 5, Mass: 10, Gravity: 3.71,         DragCoeffecient: 0.2),
        new FFWARInput(Id: 6, TimeStep: 0.001, VelocityEffectivePower: 2,  StepCount: 100000,    InitialVelocity: 50, InitialHeight: 100, Mass: 100, Gravity: 9.81,  DragCoeffecient: 0.7),
        new FFWARInput(Id: 7, TimeStep: 000.1, VelocityEffectivePower: 1,  StepCount: 100000,    InitialVelocity: 0, InitialHeight: 0, Mass: 1, Gravity: 9.81,           DragCoeffecient: 0.9),
        new FFWARInput(Id: 8, TimeStep: 000.1, VelocityEffectivePower: 2,  StepCount: 100000,  InitialVelocity: 0, InitialHeight: 0, Mass: 0.0001, Gravity: 9.81,      DragCoeffecient: 0.1),
        new FFWARInput(Id: 9, TimeStep: 000.1, VelocityEffectivePower: 1,  StepCount: 100000,  InitialVelocity: 1000, InitialHeight: 10000, Mass: 1000, Gravity: 9.81, DragCoeffecient: 0.01),
        new FFWARInput(Id: 10, TimeStep: 000.1, VelocityEffectivePower: 2,  StepCount: 100000,     InitialVelocity: 0, InitialHeight: 0, Mass: 1, Gravity: 0,          DragCoeffecient: 0.25),
    ];

    public static readonly Lazy<FreeFallWithAirResistance> Instance = new(() => new FreeFallWithAirResistance());

    public Task<FFWAROutput> Simulate(FFWARInput input, CancellationToken ct)
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
        }

        Common.Log($"{input.GetUniqueName()} processing finished!");

        return Task.FromResult(new FFWAROutput(
            Id: input.Id,
            Input: input,
            TimeSteps: timeSteps,
            Velocities: velocities,
            Positions: positions,
            DragForces: dragForces,
            NetForces: netForces));
    }

    public async IAsyncEnumerable<PlottingResult> Plot(FFWAROutput output, PlottingOptions options)
    {
        Debug.Assert(output is not null, "Output is null.");
        Debug.Assert(output.TimeSteps is not null, "TimeSteps array is null.");
        Debug.Assert(output.Velocities is not null, "Velocities array is null.");
        Debug.Assert(output.Positions is not null, "Positions array is null.");

        int n = output.Input.StepCount;
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
            Name:  $"{output.GetUniqueName()}-time-velocity",
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
            Name:  $"{output.GetUniqueName()}-time-position",
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
            Name:  $"{output.GetUniqueName()}-time-netforce",
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
            Name:  $"{output.GetUniqueName()}-time-dragforce",
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
        Debug.Assert(Inputs is not null && Inputs.Length > 0, "Simulation input list is empty or null.");
        CancellationToken ct = new();
        IEnumerable<FFWAROutput> outputs = await Common.BatchOperate(Inputs, input => Instance.Value.Simulate(input, ct));
        Debug.Assert(outputs is not null, "BatchOperate returned null.");
        await Common.WriteToJson(outputs);
    }

    public static async Task DefaultPlot()
    {
        FFWAROutput?[] outputs = Common.ReadToObject<FFWAROutput>(typeof(FFWAROutput).FullName!);
        Debug.Assert(outputs is not null, "ReadToObject returned null.");

        await Common.BatchOperate(
            outputs.Where(output => output is not null).Cast<FFWAROutput>().ToArray(),
            output => Instance.Value.Plot(output, Common.PlottingOptions));
    }
}