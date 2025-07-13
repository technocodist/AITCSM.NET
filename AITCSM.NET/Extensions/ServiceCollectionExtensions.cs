using AITCSM.NET.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AITCSM.NET.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddAITCSMServices(this IServiceCollection services)
    {
        services.AddDbContextPool<AITCSMContext>(options =>
           {
               options.UseSqlite("Data Source=c://AITCSM/AITCSM.db;");
           });
    }
}