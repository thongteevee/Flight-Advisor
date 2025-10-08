// Views/MainWindow.axaml.cs
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using FlightAdvisor.ViewModels;
using System;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using Avalonia;

namespace FlightAdvisor.Views
{
    public partial class MainWindow : Window
    {
        private WindowNotificationManager _notificationManager;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            // Setup notification manager for toast notifications
            _notificationManager = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };

            // Subscribe to weather refresh events
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.LastUpdateTime))
                    {
                        ShowWeatherUpdateNotification();
                    }
                };
            }
        }

        private void ShowWeatherUpdateNotification()
        {
            _notificationManager?.Show(new Notification(
                "Weather Updated",
                "Latest weather data has been fetched successfully.",
                NotificationType.Success,
                TimeSpan.FromSeconds(3)
            ));
        }

        private void FlightLesson_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlightAdvisor.ViewModels.MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Flight Lesson");
        }

        private void Gliding_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlightAdvisor.ViewModels.MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Gliding");
        }

        private void Recreational_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlightAdvisor.ViewModels.MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Recreational Flight");
        }

        private void Discovery_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlightAdvisor.ViewModels.MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Discovery Flight");
        }

        private void JustLooking_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlightAdvisor.ViewModels.MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Just Looking at Weather");
        }

        private async void CheckWeather_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.CheckWeatherCommand.Execute();
            }
        }

        private void ToggleAdvanced_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.ShowAdvancedMode = !vm.ShowAdvancedMode;
        }

        private void ToggleFlightDetails_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.ShowFlightDetails = !vm.ShowFlightDetails;
        }

        private void ToggleTheme_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Toggle the theme on THIS WINDOW, not the App
            var currentTheme = this.RequestedThemeVariant;
            this.RequestedThemeVariant = currentTheme == Avalonia.Styling.ThemeVariant.Dark
                ? Avalonia.Styling.ThemeVariant.Light
                : Avalonia.Styling.ThemeVariant.Dark;

            if (DataContext is MainViewModel vm)
            {
                vm.IsDarkMode = this.RequestedThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
            }
        }

        private async void SwitchToDeparture_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.SwitchToAirportAsync("Departure");
            }
        }

        private async void SwitchToArrival_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.SwitchToAirportAsync("Arrival");
            }
        }

        private async void SwitchToAlternate_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.SwitchToAirportAsync("Alternate");
            }
        }

        private async void RetryError_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.CheckWeatherCommand.Execute().Subscribe();
            }
        }

        private async void ClearError_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ClearErrorCommand.Execute().Subscribe();
            }
        }
    }
}