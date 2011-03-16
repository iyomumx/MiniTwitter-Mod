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
using System.Globalization;
namespace MiniTwitter
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void LoadLanguage()
        {
            CultureInfo currentCultureInfo = CultureInfo.CurrentCulture;
            ResourceDictionary langRd = null;
            try
            {
                langRd =
                    Application.LoadComponent(
                             new Uri(@"Resources\Translation\" + currentCultureInfo.Name + ".xaml", UriKind.Relative))
                    as ResourceDictionary;
            }
            catch
            {
                try
                {
                    langRd =
                    Application.LoadComponent(
                             new Uri(@"Resources\Translation\DefaultLanguage.xaml", UriKind.Relative))
                    as ResourceDictionary;
                }
                catch
                {
                }
            }
            if (langRd != null)
            {
                if (!(this.Resources.MergedDictionaries.Contains(langRd)))
                {
                    this.Resources.MergedDictionaries.Add(langRd);
                }
            }
        }
        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (!mutex.WaitOne(0, false))
            {
                MessageBox.Show("已经启动了一个MiniTwitter!", App.NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }
            LoadLanguage();
            var exeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            KeyMapping.LoadFrom(exeDirectory);
            ThemeManager.LoadFrom(exeDirectory);

            if (KeyMapping.KeyMappings.Count == 0)
            {
                MessageBox.Show("键盘映射文件读取失败", App.NAME);
                Shutdown();
                return;
            }

            if (ThemeManager.Themes.Count == 0)
            {
                MessageBox.Show("主题文件读取失败", App.NAME);
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

            if (Settings.Default.PlixiPassword.IsNullOrEmpty() || Settings.Default.PlixiUsername.IsNullOrEmpty())
            {
                Net.TwitterClient.googlHelper = new Google.UrlShorter.UrlShorter();
            }
            else
            {
                Net.TwitterClient.googlHelper = new Google.UrlShorter.UrlShorter("anonymous", "anonymous", Settings.Default.PlixiUsername, Settings.Default.PlixiPassword);
            }
            Net.TwitterClient.googlHelper.Key = google_key;

            MainWindow = new MainWindow();
            MainWindow.Show();
            //Log.Logger.Default.AddLogItem(new Log.LogItem("程序启动"));
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
            //Log.Logger.Default.AddLogItem(new Log.LogItem(exception));
            MessageBox.Show("发生内部错误 \n\n" + exception.ToString(), App.NAME);
            Application.Current.Shutdown();
#endif
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            //Log.Logger.Default.AddLogItem(new Log.LogItem("程序退出"));
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
            ((IDisposable)Log.Logger.Default).Dispose();
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
            //Log.Logger.Default.AddLogItem(new Log.LogItem(e.Exception));
            MessageBox.Show("发生内部错误\n\n" + e.Exception.ToString(), App.NAME);
            e.Handled = true;
            Application.Current.Shutdown();
#endif
        }

        private volatile bool _isSaved = false;

        private readonly object _syncLock = new object();
        private readonly Mutex mutex = new Mutex(false, NAME);

        private readonly string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), NAME);

        public const string VERSION = "1.66.⑨.7";
        public const string NAME = "MiniTwitter Mod";

        public const string google_key = GoogleAPIKey;
        public const string consumer_key = TwitterConsumerKey;
        public const string consumer_secret = TwitterConsumerSecret;
    }
}
