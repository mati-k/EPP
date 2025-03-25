using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;
using EPP.ViewModels;
using EPP.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;

namespace EPP
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection();
                services.AddSingleton<IFileService>(x => new FileService(desktop));
                services.AddSingleton<IGfxService, GfxService>();
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddSingleton<IFontService, FontService>();
                Ioc.Default.ConfigureServices(services.BuildServiceProvider());

                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
            else if (Design.IsDesignMode)
            {
                var services = new ServiceCollection();
                services.AddSingleton<IGfxService, MockGfxService>();
                Ioc.Default.ConfigureServices(services.BuildServiceProvider());
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}