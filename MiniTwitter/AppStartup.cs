using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPF = System.Windows;

using Microsoft.VisualBasic.ApplicationServices;

namespace MiniTwitter
{
    class AppStartup
    {
        internal static SingleInstanceApplicationWrapper wrapper;
        [STAThread()]
        static void Main(string[] args)
        {
            wrapper = new SingleInstanceApplicationWrapper();
            wrapper.Run(args);
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
                    if (UpdateRequested!=null)
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
