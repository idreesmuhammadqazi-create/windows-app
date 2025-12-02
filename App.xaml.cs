using Microsoft.Extensions.DependencyInjection;
using PseudoRun.Desktop.Services;
using PseudoRun.Desktop.ViewModels;
using PseudoRun.Desktop.Views;
using System;
using System.Windows;

namespace PseudoRun.Desktop
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IFileIOService, FileIOService>();
            services.AddSingleton<IExportService, ExportService>();
            services.AddSingleton<IExamplesService, ExamplesService>();
            services.AddSingleton<IInputService, InputService>();
            services.AddTransient<IInterpreterService, InterpreterService>();
            services.AddTransient<IValidationService, ValidationService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<EditorViewModel>();
            services.AddTransient<InterpreterViewModel>();
            services.AddTransient<DebugViewModel>();
            services.AddTransient<PracticeProblemsViewModel>();
            services.AddTransient<TutorialViewModel>();
            services.AddTransient<ExamModeViewModel>();
            services.AddTransient<ExamplesViewModel>();
            services.AddTransient<SyntaxReferenceViewModel>();

            // Windows
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (_serviceProvider != null)
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _serviceProvider?.Dispose();
        }

        public static T? GetService<T>() where T : class
        {
            return ((App)Current)._serviceProvider?.GetService<T>();
        }
    }
}
