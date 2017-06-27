using Clippy.Common;
using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace Clippy
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        static Mutex AppMutex = new Mutex(true, "Clippy-7868A4BA-474B-4E73-8442-56994430D0E2");

        [STAThread]
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!AppMutex.WaitOne(0, false))
            {
                MessageBox.Show("An instance of Clippy is already running", "Clippy", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StartProgram(e);       
        }

        private void StartProgram(StartupEventArgs e)
        {
            try
            {
                bool hasArgs = false;
                string[] args = null;
                if (e != null && e.Args.Length > 0)
                {
                    hasArgs = true;
                    args = e.Args;
                }

                MainWindow mainWindow = new MainWindow(hasArgs, args);
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown exception. Program execution aborted.");
                Console.WriteLine("Error message: " + Environment.NewLine + ex.Message);

                var Assembly = System.Reflection.Assembly.GetExecutingAssembly();
                LogfileHandler crashReport = new LogfileHandler();
                crashReport.AddDateStamp = false;
                crashReport.Filename = "CLippy-crash-report_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
                crashReport.Path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                crashReport.AddEntry("CLippy Crash report");
                crashReport.AddSpace();
                crashReport.AddEntry("Assembly information: " + Assembly.GetName().Version.ToString());
                crashReport.AddEntry("Unknown exception. Program execution aborted. Exception details:");
                crashReport.AddSpace();
                crashReport.AddEntry(Environment.NewLine + ex.Message, false);

                //Rethrow exception
                throw (ex);
            }
        }
    }
}
