using System.Windows;

namespace Matteo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Prevent multiple instances
            var processName = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (processName != null && System.Diagnostics.Process.GetProcessesByName(processName).Length > 1)
            {
                MessageBox.Show("Matteo er allerede i gang!", "Feil");
                Current.Shutdown();
                return;
            }
        }
    }
} 