// Views/MainWindow.axaml.cs
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
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

        private void FlightLesson_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectedFlightType = "Flight Lesson";
        }

        private void Gliding_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectedFlightType = "Gliding";
        }

        private void Recreational_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectedFlightType = "Recreational Flight";
        }

        private void Discovery_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectedFlightType = "Discovery Flight";
        }

        private void JustLooking_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectedFlightType = "Just Looking at Weather";
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
    }
}