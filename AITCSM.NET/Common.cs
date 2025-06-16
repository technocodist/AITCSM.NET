using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AITCSM.NET.Base;

namespace AITCSM.NET;

public static class Common
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public static string OutputDir { get; } = "./Results";

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
        where T : notnull, Identifyable
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

    public static async Task<TOut[]> BatchOperate<TIn, TOut>(TIn[] inputs, Func<TIn, TOut> @operator)
    {
        Task<TOut>[] simulations = [.. inputs.Select(
            input =>  Task.Run(() => @operator(input)))];

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