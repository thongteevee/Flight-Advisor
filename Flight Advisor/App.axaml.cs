using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FlightAdvisor.ViewModels;
using FlightAdvisor.Views;
using Avalonia.Styling;

namespace FlightAdvisor
{
    public partial class App : Application
    {

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new SplashScreen();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void ToggleTheme()
        {
            var currentTheme = RequestedThemeVariant;
            RequestedThemeVariant = currentTheme == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }

        public bool IsDarkMode => RequestedThemeVariant == ThemeVariant.Dark;

        private void DisableAvaloniaDataAnnotationValidation()
        {
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}