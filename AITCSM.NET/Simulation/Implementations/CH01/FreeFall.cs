// using AITCSM.NET.Simulation.Abstractions;
// using AITCSM.NET.Simulation.Abstractions.Entity;
// using System.Diagnostics;

// namespace AITCSM.NET.Simulation.Implementations.CH01;

// public record FFInput(
//     int Id,
//     double TimeStep,
//     int StepCount,
//     double InitialVelocity,
//     double InitialHeight,
//     double Mass,
//     double Gravity) : Identifyable(Id);

// public record FFOutput(
//     int Id,
//     FFInput Input,
//     double[] TimeSteps,
//     double[] Velocities,
//     double[] Positions) : Identifyable(Id);

// public class FreeFall : ISimulation<FFInput, FFOutput>, IPlotable<FFOutput>
// {
//     public static readonly FFInput[] Inputs = [
//         new FFInput(Id: 1, TimeStep: 0.01, StepCount: 1000, InitialVelocity: 10, InitialHeight: 0, Mass: 1, Gravity: 9.81),
//         new FFInput(Id: 2, TimeStep: 0.1, StepCount: 100, InitialVelocity: 0, InitialHeight: 100, Mass: 2, Gravity: 9.81),
//         new FFInput(Id: 3, TimeStep: 1.0, StepCount: 10, InitialVelocity: -5, InitialHeight: 50, Mass: 5, Gravity: 9.81),
//         new FFInput(Id: 4, TimeStep: 0.05, StepCount: 200, InitialVelocity: 20, InitialHeight: 10, Mass: 0.5, Gravity: 1.62),
//         new FFInput(Id: 5, TimeStep: 0.01, StepCount: 500, InitialVelocity: 15, InitialHeight: 5, Mass: 10, Gravity: 3.71),
//         new FFInput(Id: 6, TimeStep: 0.001, StepCount: 10000, InitialVelocity: 50, InitialHeight: 100, Mass: 100, Gravity: 9.81),
//         new FFInput(Id: 7, TimeStep: 0.1, StepCount: 0, InitialVelocity: 0, InitialHeight: 0, Mass: 1, Gravity: 9.81),
//         new FFInput(Id: 8, TimeStep: 0.1, StepCount: 100, InitialVelocity: 0, InitialHeight: 0, Mass: 0.0001, Gravity: 9.81),
//         new FFInput(Id: 9, TimeStep: 0.1, StepCount: 100, InitialVelocity: 1000, InitialHeight: 10000, Mass: 1000, Gravity: 9.81),
//         new FFInput(Id: 10, TimeStep: 0.1, StepCount: 100, InitialVelocity: 0, InitialHeight: 0, Mass: 1, Gravity: 0),
//     ];

//     public static readonly Lazy<FreeFall> Instance = new(() => new FreeFall());

//     public Task<FFOutput> Simulate(FFInput input, CancellationToken ct)
//     {
//         Debug.Assert(input is not null, "Input is null.");
//         Debug.Assert(input.StepCount >= 0, "StepCount must be non-negative.");
//         Debug.Assert(input.TimeStep > 0, "TimeStep must be positive.");
//         Debug.Assert(input.Mass > 0, "Mass must be positive.");
//         Debug.Assert(input.Gravity >= 0, "Gravity must be non-negative.");

//         Common.Log($"{input.GetUniqueName()} processing started!");

//         double[] TimeSteps = new double[input.StepCount];
//         double[] Velocities = new double[input.StepCount];
//         double[] Positions = new double[input.StepCount];

//         double time = 0.0;
//         double velocity = input.InitialVelocity;
//         double position = input.InitialHeight;
//         double gravityForce = -input.Mass * input.Gravity;
//         double acceleration = gravityForce / input.Mass;

//         for (int step = 0; step < input.StepCount; step++)
//         {
//             velocity += acceleration * input.TimeStep;
//             position += velocity * input.TimeStep;
//             time += input.TimeStep;

//             TimeSteps[step] = time;
//             Velocities[step] = velocity;
//             Positions[step] = position;
//         }

//         Common.Log($"{input.GetUniqueName()} processing finished!");

//         return Task.FromResult(new FFOutput(
//             Id: input.Id,
//             Input: input,
//             TimeSteps: TimeSteps,
//             Velocities: Velocities,
//             Positions: Positions));
//     }

//     public async IAsyncEnumerable<PlottingResult> Plot(FFOutput output, PlottingOptions options)
//     {
//         Debug.Assert(output is not null, "Output is null.");
//         Debug.Assert(output.TimeSteps is not null, "TimeSteps array is null.");
//         Debug.Assert(output.Velocities is not null, "Velocities array is null.");
//         Debug.Assert(output.Positions is not null, "Positions array is null.");

//         int n = output.Input.StepCount;
//         Debug.Assert(output.TimeSteps.Length == n, $"TimeSteps.Length != StepCount ({n})");
//         Debug.Assert(output.Velocities.Length == n, $"Velocities.Length != StepCount ({n})");
//         Debug.Assert(output.Positions.Length == n, $"Positions.Length != StepCount ({n})");

//         Common.Log($"Plotting {output.GetUniqueName()} started!");

//         ScottPlot.Plot plt = new();

//         plt.Add.Scatter(output.TimeSteps, output.Velocities);
//         plt.Title("Time vs. Velocity Diagram");
//         plt.XLabel("Time Steps");
//         plt.YLabel("Velocities");

//         yield return new PlottingResult(
//             Name: $"{output.GetUniqueName()}-time-velocity",
//             ImageBytes: plt.GetImage(options.Width, options.Height).GetImageBytes(),
//             Format: options.Format,
//             Width: options.Width,
//             Height: options.Height
//         );

//         plt.Clear();
//         plt.Add.Scatter(output.TimeSteps, output.Positions);
//         plt.Title("Time vs. Position Diagram");
//         plt.XLabel("Time Steps");
//         plt.YLabel("Positions");

//         yield return new PlottingResult(
//             Name: $"{output.GetUniqueName()}-time-position",
//             ImageBytes: plt.GetImage(options.Width, options.Height).GetImageBytes(),
//             Format: options.Format,
//             Width: options.Width,
//             Height: options.Height
//         );

//         Common.Log($"Plotting {output.GetUniqueName()} finished!");
//         await Task.CompletedTask;
//     }

//     public static async Task DefaultSimulate()
//     {
//         Debug.Assert(Inputs is not null && Inputs.Length > 0, "Simulation input list is empty or null.");
//         CancellationToken ct = new();
//         IEnumerable<FFOutput> outputs = await Common.BatchOperate(Inputs, input => Instance.Value.Simulate(input, ct));
//         Debug.Assert(outputs is not null, "BatchOperate returned null.");
//         await Common.WriteToJson(outputs);
//     }

//     public static async Task DefaultPlot()
//     {
//         FFOutput?[] outputs = Common.ReadToObject<FFOutput>(typeof(FFOutput).FullName!);
//         Debug.Assert(outputs is not null, "ReadToObject returned null.");

//         await Common.BatchOperate(
//             outputs.Where(output => output is not null).Cast<FFOutput>().ToArray(),
//             output => Instance.Value.Plot(output, Common.PlottingOptions));
//     }
// }