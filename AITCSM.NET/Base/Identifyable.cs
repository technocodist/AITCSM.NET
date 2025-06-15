namespace AITCSM.NET.Base;

public record Identifyable(int Id)
{
    public string GetUniqueName()
    {
        return $"{GetType().FullName}_{Id}";
    }
}