using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Deployment.Application;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using MiniTwitter.Extensions;

namespace MiniTwitter.Update
{
    /// <summary>
    /// UpdateDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateDialog : Window
    {
        public UpdateDialog()
        {
            InitializeComponent();
            MainTimer.Interval = TimeSpan.FromMilliseconds(500);
            MainTimer.Tick += new EventHandler(MainTimer_Tick);
        }

        void MainTimer_Tick(object sender, EventArgs e)
        {
            var s = StatusText;
            if (s.EndsWith("..."))
            {
                StatusText = s.Substring(0, s.Length - 3);
            }
            else
            {
                StatusText = s + ".";
            }
        }
        public static readonly DependencyProperty StatusTextProperty
            = DependencyProperty.Register("StatusText",
                typeof(string), typeof(UpdateDialog),
                new FrameworkPropertyMetadata("..."));
        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        public static readonly DependencyProperty UpdateAvaliableProperty
            = DependencyProperty.Register("UpdateAvaliable",
                typeof(bool), typeof(UpdateDialog),
                new FrameworkPropertyMetadata(false));
        public bool UpdateAvaliable
        {
            get { return (bool)GetValue(UpdateAvaliableProperty); }
            set { SetValue(UpdateAvaliableProperty, value); }
        }

        public static readonly DependencyProperty UpdateSizeProperty
            = DependencyProperty.Register("UpdateSize",
                typeof(long), typeof(UpdateDialog),
                new FrameworkPropertyMetadata(0L));
        public long UpdateSize
        {
            get { return (long)GetValue(UpdateSizeProperty); }
            set { SetValue(UpdateSizeProperty, value); }
        }

        public static readonly DependencyProperty IsRequiredUpdateProperty
            = DependencyProperty.Register("IsRequiredUpdate",
                typeof(bool), typeof(UpdateDialog),
                new FrameworkPropertyMetadata(false));
        public bool IsRequiredUpdate
        {
            get { return (bool)GetValue(IsRequiredUpdateProperty); }
            set { SetValue(IsRequiredUpdateProperty, value); }
        }

        public static readonly DependencyProperty UpdateVersionProperty
            = DependencyProperty.Register("UpdateVersion",
                typeof(Version), typeof(UpdateDialog),
                new FrameworkPropertyMetadata(ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Version.Parse("1.0.0.0")));
        public Version UpdateVersion
        {
            get { return (Version)GetValue(UpdateVersionProperty); }
            set { SetValue(UpdateVersionProperty, value); }
        }

        public static readonly DependencyProperty MiniorVersionProperty
            = DependencyProperty.Register("MiniorVersion",
                typeof(Version), typeof(UpdateDialog),
                new FrameworkPropertyMetadata(Version.Parse("1.0.0.0")));
        public Version MiniorVersion
        {
            get { return (Version)GetValue(MiniorVersionProperty); }
            set { SetValue(MiniorVersionProperty, value); }
        }

        public static readonly DependencyProperty UpdateProgressProperty
            = DependencyProperty.Register("UpdateProgress",
                typeof(double), typeof(UpdateDialog),
                new FrameworkPropertyMetadata(0));
        public double UpdateProgress
        {
            get { return (double)GetValue(UpdateProgressProperty); }
            set { SetValue(UpdateProgressProperty, value); }
        }

        private ApplicationDeployment deploy = ApplicationDeployment.CurrentDeployment;

        private DispatcherTimer MainTimer = new DispatcherTimer(DispatcherPriority.Background);

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            deploy.CheckForUpdateAsync();
            deploy.CheckForUpdateProgressChanged += new DeploymentProgressChangedEventHandler(deploy_CheckForUpdateProgressChanged);
            deploy.CheckForUpdateCompleted += new CheckForUpdateCompletedEventHandler(deploy_CheckForUpdateCompleted);
        }

        void deploy_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            if (MessageBox.Show("找到可用更新，是否立即更新？", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var detail = deploy.CheckForDetailedUpdate();
                
                deploy.UpdateProgressChanged += new DeploymentProgressChangedEventHandler(deploy_UpdateProgressChanged);
                deploy.UpdateAsync();
                deploy.UpdateCompleted += new System.ComponentModel.AsyncCompletedEventHandler(deploy_UpdateCompleted);
            }
        }

        void deploy_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void deploy_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void deploy_CheckForUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
