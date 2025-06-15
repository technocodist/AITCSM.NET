using System.Text.Json;
using System.Text.Json.Serialization;
using AITCSM.NET.Base;

namespace AITCSM.NET;

public static class Common
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new() {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };

    public static string OutputDir { get; } = "Results";

    public static async Task WriteToJson<T>(IEnumerable<T> inputs)
        where T : notnull, Identifyable
    {
        if (!Directory.Exists(OutputDir))
        {
            Directory.CreateDirectory(OutputDir);
        }

        Task[] tasks = [.. inputs.Select(input => Task.Run(() =>
        {
            File.WriteAllText(
                Path.Combine(OutputDir, $"{typeof(T).FullName}_{input.Id}.json"),
                JsonSerializer.Serialize(input, JsonSerializerOptions));
        }))];

        await Task.WhenAll(tasks);
    }

    public static async Task<TOut[]> BatchSimulate<TIn, TOut>(TIn[] inputs, Func<TIn, TOut> simulator)
    {
        Task<TOut>[] simulations = [.. inputs.Select(
            input =>  Task.Run(() => simulator(input)))];

        return await Task.WhenAll(simulations);
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