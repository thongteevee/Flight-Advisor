// Views/SplashScreen.axaml.cs
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace FlightAdvisor.Views
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            // Auto-close splash screen after 3 seconds
            StartSplashTimer();
        }

        private async void StartSplashTimer()
        {
            await Task.Delay(3000); // Show for 3 seconds

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Open main window
                var mainWindow = new MainWindow();
                mainWindow.Show();

                // Close splash screen
                this.Close();
            });
        }
    }
}
