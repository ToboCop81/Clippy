/// Clippy - File: "App.xaml.cs"
/// Copyright © 2021 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using System;
using System.Diagnostics;
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
            try
            {
                bool hasArgs = false;
                string[] args = null;
                if (e != null && e.Args.Length > 0)
                {
                    hasArgs = true;
                    args = e.Args;
                }

                if (!AppMutex.WaitOne(0, false))
                {
                    Functionality.AsyncPlipeClient clippyPipeclient = new Functionality.AsyncPlipeClient();
                    string pipeName = StaticHelper.GetPipeName();

                    if (!hasArgs)
                    {
                        clippyPipeclient.SendMessage(StaticHelper.Base64Encode(":BRINGTOFRONT:"), pipeName);
                        Debug.WriteLine("[Client] Sent ':BRINGTOFRONT:' to running instance");

                    }
                    else
                    {
                        string fileName = string.Join(" ", args).Trim();
                        // Check if first argument is a valid file name - then send it to the already running instance and exit
                        if (File.Exists(fileName))
                        {
                            fileName = StaticHelper.Base64Encode(fileName);
                            clippyPipeclient.SendMessage(fileName, pipeName);
                            Debug.WriteLine("[Client] Sent filename to running instance.");

                        }
                    }

                    clippyPipeclient = null;
                    Environment.Exit(Environment.ExitCode);
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
                crashReport.Filename = "Clippy-crash-report_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
                crashReport.Path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                crashReport.AddEntry("Clippy Crash report");
                crashReport.AddSpace();
                crashReport.AddEntry($"Assembly information: {Assembly.GetName().Version.ToString()}");
                crashReport.AddEntry($"OS Version: {Environment.OSVersion.VersionString}");
                crashReport.AddEntry("Unknown exception. Program execution aborted. Exception details:");
                crashReport.AddSpace();
                crashReport.AddEntry(Environment.NewLine + ex.Message, false);
                crashReport.AddSpace();
                crashReport.AddEntry("Call stack:");
                crashReport.AddEntry(ex.StackTrace);

                if (ex is AbandonedMutexException)
                {
                    AppMutex.ReleaseMutex();
                    crashReport.AddEntry("Released abandoned mutex");
                }

                //Rethrow exception
                throw (ex);
            }
        }
    }

}
