// Views/ErrorDisplay.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using FlightAdvisor.ViewModels;
using System;

namespace FlightAdvisor.Views
{
    public partial class ErrorDisplay : UserControl
    {
        public ErrorDisplay()
        {
            InitializeComponent();
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Execute the command without await
                vm.CheckWeatherCommand.Execute().Subscribe();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ClearErrorCommand.Execute().Subscribe();
            }
        }
    }
}