namespace AITCSM.NET;

public static class DI
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider is not initialized. Call InitializeServiceProvider first.");
            }
            return _serviceProvider;
        }

        set
        {
            if (_serviceProvider != null)
            {
                throw new InvalidOperationException("ServiceProvider is already initialized.");
            }
            
            _serviceProvider = value;
        }
    }
}