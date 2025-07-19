using AITCSM.NET.Data.Entities.Abstractions;
using AITCSM.NET.Simulation.Abstractions;
using ScottPlot;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace AITCSM.NET;

public static class Common
{
    public static readonly int BatchSize = 10_000;

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public static string OutputDir { get; } = "./Results";

    public static PlottingOptions PlottingOptions { get; } = new(
        Format: ImageFormat.Png,
        Width: 720,
        Height: 480
    );

    public static TOut?[] ReadToObject<TOut>(string name)
    where TOut : class
    {
        if (!Directory.Exists(OutputDir))
        {
            Directory.CreateDirectory(OutputDir);
        }

        string[] files = [.. Directory.GetFiles(OutputDir).Where(fileName => fileName.Contains(name) && fileName.EndsWith(".json"))];

        return [.. files
            .Select(filePath => JsonSerializer
                .Deserialize<TOut>(
                    File.ReadAllText(filePath),
                    JsonSerializerOptions))];
    }

    public static async Task WriteToJson<T>(IEnumerable<T> inputs)
        where T : notnull, EntityBase
    {
        if (!Directory.Exists(OutputDir))
        {
            Directory.CreateDirectory(OutputDir);
        }

        Task[] tasks = [.. inputs.Select(input => Task.Run(() =>
        {
            File.WriteAllText(
                Path.Combine(OutputDir, $"{input.GetUniqueName()}.json"),
                JsonSerializer.Serialize(input, JsonSerializerOptions));
        }))];

        await Task.WhenAll(tasks);
    }

    public static async IAsyncEnumerable<TOut> RunConcurrentSimulations<TIn, TOut>(
        this ISimulation<TIn, TOut> simulator,
        IEnumerable<TIn> inputs,
        int degreeOfParallelism,
        int channelCapacity = 5_000,
        [EnumeratorCancellation] CancellationToken ct = default)
        where TIn : EntityBase
        where TOut : EntityBase
    {
        Channel<TOut> channel = Channel.CreateBounded<TOut>(new BoundedChannelOptions(channelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        SemaphoreSlim throttler = new(degreeOfParallelism);

        List<Task> tasks = [.. inputs.Select(async input =>
            {
                await throttler.WaitAsync(ct);

                try
                {
                    await foreach (TOut result in simulator.Simulate(input, ct).WithCancellation(ct))
                    {
                        await channel.Writer.WriteAsync(result, ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                }
                catch (Exception ex)
                {
                    channel.Writer.TryComplete(ex);
                    throw;
                }
                finally
                {
                    throttler.Release();
                }
            }
        )];

        _ = Task.WhenAll(tasks).ContinueWith(task =>
        {
            channel.Writer.TryComplete(task.Exception);
        }, ct);

        await foreach (TOut item in channel.Reader.ReadAllAsync(ct))
        {
            yield return item;
        }
    }



    public static async Task<TOut[]> BatchOperate<TIn, TOut>(TIn[] inputs, Func<TIn, TOut> @operator)
    {
        Task<TOut>[] simulations = [.. inputs.Select(
            input =>  Task.Run(() => @operator(input)))];

        return await Task.WhenAll(simulations);
    }

    public static async Task<TOut[]> BatchOperate<TIn, TOut>(TIn[] inputs, Func<TIn, Task<TOut>> @operator)
    {
        Task<TOut>[] simulations = [.. inputs.Select(input => @operator(input))];

        return await Task.WhenAll(simulations);
    }

    public static async Task BatchOperate<TIn>(TIn[] inputs, Action<TIn> @operator)
    {
        Task[] simulations = [.. inputs.Select(
            input =>  Task.Run(() => @operator(input)))];

        await Task.WhenAll(simulations);
    }

    public static void Log(string format, params object?[]? objects)
    {
        Console.WriteLine(format, objects);
    }

    public static void LogIf(bool condition, string format, params object?[]? objects)
    {
        if (condition)
        {
            Console.WriteLine(format, objects);
        }
    }
}