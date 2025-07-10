using AITCSM.NET.Simulation.Abstractions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Reflection;

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
        List<Type> types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == simType) && !t.IsInterface && !t.IsAbstract)
            .ToList();
        SimulationListBox.ItemsSource = types.Select(t => t.Name).ToList();
    }

    private void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        string? selected = SimulationListBox.SelectedItem as string;
        if (selected == null)
        {
            ResultTextBlock.Text = "Please select a simulation.";
            return;
        }
        ResultTextBlock.Text = $"Simulation '{selected}' started (stub).";
        // TODO: Add input, start/stop, and database logic
    }
}