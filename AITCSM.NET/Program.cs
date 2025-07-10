using AITCSM.NET.UI;
using Avalonia;

namespace AITCSM.NET;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}