using AITCSM.NET.Simulation.Abstractions.Entity;
using ScottPlot;

namespace AITCSM.NET.Simulation.Abstractions;

public record struct PlottingResult(
    string Name,
    byte[] ImageBytes,
    ImageFormat Format,
    int Width,
    int Height
);

public record struct PlottingOptions(
    ImageFormat Format,
    int Width,
    int Height
);

public interface IPlotable<TOut>
    where TOut : Identifyable
{
    IAsyncEnumerable<PlottingResult> Plot(TOut output, PlottingOptions options);
}