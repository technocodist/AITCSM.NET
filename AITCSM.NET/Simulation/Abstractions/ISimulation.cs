using AITCSM.NET.Simulation.Abstractions.Entity;

namespace AITCSM.NET.Simulation.Abstractions
{
    public interface ISimulation<TIn, TOut>
        where TIn : Identifyable
        where TOut : Identifyable
    {
        Task<TOut> Simulate(TIn input, CancellationToken ct);
    }
}