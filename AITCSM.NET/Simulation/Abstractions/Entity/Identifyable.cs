namespace AITCSM.NET.Simulation.Abstractions.Entity;

public record Identifyable(int Id)
{
    public string GetUniqueName()
    {
        return $"{GetType().FullName}_{Id}";
    }
}