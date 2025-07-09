using AITCSM.NET.Data.Entities.Abstractions;

namespace AITCSM.NET.Simulation.Abstractions;

public interface ISimulation<TIn, TOut>
    where TIn : EntityBase
    where TOut : EntityBase
{
    IAsyncEnumerable<TOut> Simulate(TIn input, CancellationToken ct);
}