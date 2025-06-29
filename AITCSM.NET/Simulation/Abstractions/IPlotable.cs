using AITCSM.NET.Abstractions.Entity;
using ScottPlot;

namespace AITCSM.NET.Abstractions;

public record class PlottingOptions(
    string OutputDirectory,
    ImageFormat Format,
    int Width,
    int Height
);

public interface IPlotable<TOut>
    where TOut : Identifyable
{
    Task Plot(TOut output, PlottingOptions options);
}