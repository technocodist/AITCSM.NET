using AITCSM.NET.Simulation.Abstractions;
using AITCSM.NET.Simulation.Implementations.CH01;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Reflection;
using System.Threading.Tasks;

namespace AITCSM.NET.UI;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
        Opened += MainWindow_Opened;
        StartButton.Click += StartButton_Click;
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        // Use reflection to find all ISimulation<,> implementations
        Type simType = typeof(ISimulation<,>);
        List<Type> types = [.. Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == simType) && !t.IsInterface && !t.IsAbstract)];
        SimulationListBox.ItemsSource = types.Select(t => t.Name).ToList();
    }

    private async void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        if (SimulationListBox.SelectedItem is not string selected)
        {
            ResultTextBlock.Text = "Please select a simulation.";
            return;
        }
        ResultTextBlock.Text = $"Simulation '{selected}' started (stub).";

        await Dispatcher.UIThread.InvokeAsync(DistributionOfMoneySimulation.DefaultSimulate);
    }
}