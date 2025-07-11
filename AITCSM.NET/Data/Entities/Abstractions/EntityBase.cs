namespace AITCSM.NET.Data.Entities.Abstractions;

public class EntityBase
{
    public int Id { get; set; }

    public string GetUniqueName()
    {
        return $"{GetType().FullName}_{Id}";
    }
}