// Views/MainWindow.axaml.cs - Complete file with cleaned theme logic
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Styling;
using FlightAdvisor.ViewModels;
using System;
using System.Reactive.Linq;

namespace FlightAdvisor.Views
{
    public partial class MainWindow : Window
    {
        private WindowNotificationManager _notificationManager;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            // Initialize theme to match ViewModel default
            if (DataContext is MainViewModel viewModel)
            {
                ApplyTheme(viewModel.IsDarkMode);

                // Subscribe to theme changes from ViewModel
                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.IsDarkMode))
                    {
                        ApplyTheme(viewModel.IsDarkMode);
                    }
                    else if (e.PropertyName == nameof(MainViewModel.LastUpdateTime))
                    {
                        ShowWeatherUpdateNotification();
                    }
                };
            }

            // Setup notification manager for toast notifications
            _notificationManager = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };
        }

        /// <summary>
        /// Applies the theme to the window
        /// </summary>
        private void ApplyTheme(bool isDarkMode)
        {
            this.RequestedThemeVariant = isDarkMode
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }

        /// <summary>
        /// Theme toggle button click handler
        /// </summary>
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ToggleTheme();
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

        // Keep all your existing event handlers below
        private void FlightLesson_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Flight Lesson");
        }

        private void Gliding_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Gliding");
        }

        private void Recreational_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Recreational Flight");
        }

        private void Discovery_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Discovery Flight");
        }

        private void JustLooking_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectFlightTypeCommand.Execute("Just Looking at Weather");
        }

        private async void CheckWeather_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.CheckWeatherCommand.Execute();
            }
        }

        private void ToggleAdvanced_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.ShowAdvancedMode = !vm.ShowAdvancedMode;
        }

        private void ToggleFlightDetails_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.ShowFlightDetails = !vm.ShowFlightDetails;
        }

        private async void SwitchToDeparture_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.SwitchToAirportAsync("Departure");
            }
        }

        private async void SwitchToArrival_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.SwitchToAirportAsync("Arrival");
            }
        }

        private async void SwitchToAlternate_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.SwitchToAirportAsync("Alternate");
            }
        }
    }
}