using System.Windows;

namespace DateiverwaltungApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Globale Ausnahmebehandlung
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(
                    $"Ein unerwarteter Fehler ist aufgetreten:\n{args.Exception.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                args.Handled = true;
            };
        }
    }
}