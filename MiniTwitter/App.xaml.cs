using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;

using MiniTwitter.Extensions;
using MiniTwitter.Input;
using MiniTwitter.Themes;
using MiniTwitter.Properties;

namespace MiniTwitter
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (!mutex.WaitOne(0, false))
            {
                MessageBox.Show("既に起動しています。", App.NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            var exeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            KeyMapping.LoadFrom(exeDirectory);
            ThemeManager.LoadFrom(exeDirectory);

            if (KeyMapping.KeyMappings.Count == 0)
            {
                MessageBox.Show("キーボードマッピングの読み込みに失敗しました。", App.NAME);
                Shutdown();
                return;
            }

            if (ThemeManager.Themes.Count == 0)
            {
                MessageBox.Show("テーマの読み込みに失敗しました。", App.NAME);
                Shutdown();
                return;
            }

            // 設定を読み込む
            Settings.Load(directory);

            if (Settings.Default.Theme.IsNullOrEmpty() || !ThemeManager.Themes.ContainsValue(Settings.Default.Theme))
            {
                // デフォルトのテーマ
                Settings.Default.Theme = ThemeManager.GetTheme(0);
            }

            this.ApplyTheme(Settings.Default.Theme);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _isSaved = true;
#if !DEBUG
            if (e.ExceptionObject == null)
            {
                Application.Current.Shutdown();
                return;
            }

            var exception = (Exception)e.ExceptionObject;
            MessageBox.Show("内部エラーが発生しました。\n\n" + exception.ToString(), App.NAME);
            Application.Current.Shutdown();
#endif
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (!_isSaved)
            {
                lock (_syncLock)
                {
                    if (!_isSaved)
                    {
                        Settings.Save();
                        _isSaved = true;
                    }
                }
            }
        }

        private void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            if (!_isSaved)
            {
                lock (_syncLock)
                {
                    if (!_isSaved)
                    {
                        Settings.Save();
                        _isSaved = true;
                    }
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _isSaved = true;
#if !DEBUG
            if (e.Exception == null)
            {
                Application.Current.Shutdown();
                return;
            }

            MessageBox.Show("内部エラーが発生しました。\n\n" + e.Exception.ToString(), App.NAME);
            e.Handled = true;
            Application.Current.Shutdown();
#endif
        }

        private volatile bool _isSaved = false;

        private readonly object _syncLock = new object();
        private readonly Mutex mutex = new Mutex(false, NAME);

        private readonly string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), NAME);

        public const string VERSION = "1.66";
        public const string NAME = "MiniTwitter";
    }
}
