using AITCSM.NET.Abstractions.Entity;

namespace AITCSM.NET.Abstractions
{
    public interface ISimulation<TIn , TOut>
        where TIn : Identifyable
        where TOut : Identifyable
    {
        Task<TOut> Simulate(TIn input);
    }
}