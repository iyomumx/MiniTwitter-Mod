using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.ApplicationServices;
using WPF = System.Windows;
using System.Reflection;
using System.Security.Principal;

namespace MiniTwitter
{
    class AppStartup
    {
        internal static SingleInstanceApplicationWrapper wrapper;
        [STAThread()]
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] != null && args[0].ToUpper() == "NGEN")
            {
                try
                {
                    if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        DoNgenWorkaround();
                    }
                }
                catch { }
                return;
            }
            wrapper = new SingleInstanceApplicationWrapper();
            wrapper.Run(args);
        }

        static void DoNgenWorkaround()
        {
            bool Success = false;
            //if (ApplicationDeployment.IsNetworkDeployed)
            {
                string appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string winPath = Environment.GetEnvironmentVariable("WINDIR");
                string ngenPath = GetNgenPath(winPath);
                using (Process proc = new Process())
                {
                    System.IO.Directory.SetCurrentDirectory(appPath);

                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.CreateNoWindow = false;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    proc.StartInfo.FileName = ngenPath;
                    proc.StartInfo.Arguments = "uninstall MiniTwitter.exe /nologo /silent";

                    proc.Start();
                    proc.WaitForExit();

                    proc.StartInfo.FileName = ngenPath;
                    proc.StartInfo.Arguments = "install MiniTwitter.exe /nologo /silent";

                    proc.Start();
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        Success = true;
                    }
                }
            }
            WPF.MessageBox.Show(string.Format("本机映像生成{0}！", Success ? "成功" : "失败"), App.NAME, WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Information);
        }

        static string GetNgenPath(string winPath)
        {
            var arch = Environment.Is64BitProcess ? "64" : "";
            var basePath = Path.Combine(winPath, @"Microsoft.NET\Framework" + arch);
            if (!Directory.Exists(basePath))
            {
                basePath = Path.Combine(winPath, @"Microsoft.NET\Framework");
                if (!Directory.Exists(basePath))
                {
                    return null;
                }
            }
            var v4dir = from dir in Directory.GetDirectories(basePath).AsParallel() where Path.GetFileName(dir).StartsWith("v4") select Path.Combine(dir, "ngen.exe");

            return v4dir.FirstOrDefault(dir => File.Exists(dir));
        }
    }

    public class SingleInstanceApplicationWrapper : WindowsFormsApplicationBase
    {
        private App app;

        public SingleInstanceApplicationWrapper()
        {
            this.IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs e)
        {
            app = new App();
            if (e.CommandLine != null && e.CommandLine.Count != 0)
            {
                if (System.IO.Directory.Exists(e.CommandLine[0]))
                {
                    app.SettingDirectory = e.CommandLine[0];
                }
            }
            app.InitializeComponent();
            app.Run();

            return false;
        }

        protected override void OnStartupNextInstance(Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
        {
            if (e.CommandLine.Count == 0)
            {
                WPF.MessageBox.Show("已经启动了一个MiniTwitter!", App.NAME, WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Information);
            }
            else
            {
                if (e.CommandLine.Select(arg => arg.ToLower()).Contains("-refresh"))
                {
                    if (RefreshRequested != null)
                    {
                        RefreshRequested(this, EventArgs.Empty);
                    }
                }
                if (e.CommandLine.Select(arg => arg.ToLower()).Contains("-update"))
                {
                    if (UpdateRequested != null)
                    {
                        UpdateRequested(this, EventArgs.Empty);
                    }
                }
                if (e.CommandLine.Select(arg => arg.ToLower()).Contains("-updatemedia"))
                {
                    if (UpdateMediaRequested != null)
                    {
                        UpdateMediaRequested(this, EventArgs.Empty);
                    }
                }
                if (e.CommandLine.Select(arg => arg.ToLower()).Contains("-settings"))
                {
                    if (SettingRequested != null)
                    {
                        SettingRequested(this, EventArgs.Empty);
                    }
                }
                if (e.CommandLine.Select(arg => arg.ToLower()).Contains("-theme"))
                {
                    var index = e.CommandLine.Select(arg => arg.ToLower()).ToList().IndexOf("-theme");
                    if (index + 1 >= e.CommandLine.Count)
                    {
                        return;
                    }
                    var tk = e.CommandLine[e.CommandLine.Select(arg => arg.ToLower()).ToList().IndexOf("-theme") + 1];
                    if (ChangeThemeRequested != null)
                    {
                        ChangeThemeRequested(this, new ChangeThemeRequestEventArgs(tk));
                    }
                }
            }
        }

        public event EventHandler RefreshRequested;
        public event EventHandler UpdateRequested;
        public event EventHandler UpdateMediaRequested;
        public event EventHandler SettingRequested;
        public event EventHandler<ChangeThemeRequestEventArgs> ChangeThemeRequested;
    }
}
