using AITCSM.NET.Simulation.Abstractions;
using AITCSM.NET.Simulation.Implementations.CH01;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Reflection;

namespace AITCSM.NET.UI;

public partial class MainWindow : Window
{
    private static readonly Dictionary<string, Func<Task>> SimulationHandlers = new()
    {
        {typeof(DistributionOfMoneySimulation).Name,  DistributionOfMoneySimulation.DefaultSimulate},
        {typeof(DistributionOfMoneyWithSavingSimulation).Name,  DistributionOfMoneyWithSavingSimulation.DefaultSimulate},
        {typeof(FreeFallSimulation).Name,  FreeFallSimulation.DefaultSimulate},
        {typeof(FreeFallWithAirResistanceSimulation).Name,  FreeFallWithAirResistanceSimulation.DefaultSimulate},
    };

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
        SimulationListBox.ItemsSource = SimulationHandlers.Keys;
    }

    private async void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        if (SimulationListBox.SelectedItem is not string selected)
        {
            ResultTextBlock.Text = "Please select a simulation.";
            return;
        }

        ResultTextBlock.Text = $"Simulation '{selected}' started (stub).";

        await Dispatcher.UIThread.InvokeAsync(SimulationHandlers[selected]);
    }
}