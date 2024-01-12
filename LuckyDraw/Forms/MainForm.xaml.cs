using LuckyDraw.Language;
using LuckyDraw.Model;
using LuckyDraw.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LuckyDraw.Forms
{
    /// <summary>
    /// MainForm.xaml 的交互逻辑
    /// </summary>
    public partial class MainForm : Window
    {
        #region Properties
        public static readonly DependencyProperty StopIsEnableProperty;

        public static readonly DependencyProperty StartIsEnableProperty;

        public static readonly DependencyProperty WelcomeForegroundProperty;

        public static readonly DependencyProperty BackgroudProperty;

        public static readonly DependencyProperty AwardConfigurationTipProperty;

        public static readonly DependencyProperty TitleBarVisibilityProperty;
        #endregion

        private AppSetting appSetting;

        private System.Timers.Timer timer;

        private List<Staff> users;

        private Random random;

        private AwardConfiguration currentShow;

        private int currentCount = 0;

        private List<AwardConfiguration> awardConfigurations;

        private String winnerListFile = "WinnerList.txt";

        private bool repeatParticipation;

        static MainForm()
        {
            StopIsEnableProperty = DependencyProperty.Register("StopIsEnable", typeof(bool), typeof(MainForm));
            StartIsEnableProperty = DependencyProperty.Register("StartIsEnable", typeof(bool), typeof(MainForm));
            WelcomeForegroundProperty = DependencyProperty.Register("WelcomeForeground", typeof(Brush), typeof(MainForm));
            BackgroudProperty = DependencyProperty.Register("Backgroud", typeof(ImageSource), typeof(MainForm));
            AwardConfigurationTipProperty = DependencyProperty.Register("AwardConfigurationTip", typeof(String), typeof(MainForm));
            TitleBarVisibilityProperty= DependencyProperty.Register("TitleBarVisibility", typeof(Visibility), typeof(MainForm));
        }
            
        public MainForm()
        {
            loadAppConfig();
            InitializeComponent();
        }

        private void loadAppConfig()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppConfig.json");
            appSetting = new AppSetting(path);
            winnerListFile = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyyyMMddHHmmss") + winnerListFile;
            initTimer();
            loadUser();
            loadAwardConfiguration(0);
            repeatParticipation = bool.Parse(appSetting["RepeatParticipation"]);
            check();
        }

        private void check()
        {
            if (!repeatParticipation)
            {
                int awardNumberCount = awardConfigurations.Sum(e => e.Number);
                if (users.Count < awardNumberCount)
                {
                    MessageBox.Show(String.Format(LanguageManager.Instance["ImportStaffConfigTip"], awardNumberCount, users.Count),
                 LanguageManager.Instance["Tip"], MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }
            }
        }

        private void loadAwardConfiguration(int currentCount)
        {
            awardConfigurations ??= appSetting.getObjcet<List<AwardConfiguration>>("AwardConfigurations");
            if (currentShow != null)
            {
                awardConfigurations.Remove(currentShow);
            }
            this.currentCount = currentCount;
            currentShow = awardConfigurations.OrderBy(e => e.Order).FirstOrDefault();
            AwardConfigurationTip = String.Format(LanguageManager.Instance["AwardConfigurationTip"]
                , currentShow.Label, currentCount, currentShow.Number);
        }

        private void loadUser()
        {
            users = new List<Staff>();
            String file = AppDomain.CurrentDomain.BaseDirectory + "user.csv";
            using StreamReader reader = new(file, Encoding.UTF8);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var arr = line.Trim().Split(',');
                users.Add(new Staff
                {
                    Name = arr[0],
                    No = arr[1]
                });
            }
            // 去掉标题
            users.RemoveAt(0);
            MessageBox.Show(LanguageManager.Instance["StaffConfigLoaded"],
                LanguageManager.Instance["Tip"], MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void initTimer()
        {
            String refreshFrequency = appSetting["WelcomeRefreshFrequency"];
            int interval;
            if (!int.TryParse(refreshFrequency, out interval))
            {
                interval = 200;
            }
            random = new Random();
            timer = new System.Timers.Timer();
            timer.Interval = interval;
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int randomNumber = random.Next(0, users.Count);
            Dispatcher.Invoke(() =>
            {
                if (users.Count > 0)
                {
                    userTb.Text = $"{users[randomNumber].Name}({users[randomNumber].No})";
                }
            });
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            bool titleBarVisibility = bool.Parse(appSetting["TitleBarVisibility"]);
            if (titleBarVisibility)
            {
                titleBar.MinimizeButtonToolTip = LanguageManager.Instance["MinimizeBtnName"];
                titleBar.MaximizeButtonToolTip = LanguageManager.Instance["MaximizeBtnName"];
                titleBar.CloseButtonToolTip = LanguageManager.Instance["CloseBtnName"];
            }
            TitleBarVisibility = titleBarVisibility ? Visibility.Visible : Visibility.Collapsed;
            StartIsEnable = true;
            StopIsEnable = false;
            var rgbStr = appSetting["WelcomeForeground"].Split(',');
            WelcomeForeground = new SolidColorBrush(Color.FromRgb(byte.Parse(rgbStr[0]), byte.Parse(rgbStr[1]), byte.Parse(rgbStr[2])));
            String backgroudImg = appSetting["Background"];
            String backgroudImgPath = AppDomain.CurrentDomain.BaseDirectory + backgroudImg;
            if (File.Exists(backgroudImgPath))
            {
                Backgroud = new BitmapImage(new Uri(backgroudImgPath));
            }
            else
            {
                MessageBox.Show(String.Format(LanguageManager.Instance["BackgroudNotFound"], backgroudImgPath,"\r\n"), LanguageManager.Instance["Tip"],
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                Backgroud = new BitmapImage(new Uri("pack://application:,,,/Resources/img/elder-tree-er.jpg"));
            }
        }

        #region Properties
        public bool StopIsEnable
        {
            get
            {
                return (bool)GetValue(StopIsEnableProperty);
            }
            set
            {
                SetValue(StopIsEnableProperty, value);
            }
        }

        public bool StartIsEnable
        {
            get
            {
                return (bool)GetValue(StartIsEnableProperty);
            }
            set
            {
                SetValue(StartIsEnableProperty, value);
            }
        }

        public String WelcomeStr
        {
            get
            {
                return appSetting["Welcome"];
            }
        }

        public Brush WelcomeForeground
        {
            get
            {
                return (Brush)GetValue(WelcomeForegroundProperty);
            }
            set
            {
                SetValue(WelcomeForegroundProperty, value);
            }
        }

        public BitmapImage Backgroud
        {
            get
            {
                return (BitmapImage)GetValue(BackgroudProperty);
            }
            set
            {
                SetValue(BackgroudProperty, value);
            }
        }

        public String AwardConfigurationTip
        {
            get
            {
                return (String)GetValue(AwardConfigurationTipProperty);
            }
            set
            {
                SetValue(AwardConfigurationTipProperty, value);
            }
        }

        public Visibility TitleBarVisibility
        {
            get
            {
                return (Visibility)GetValue(TitleBarVisibilityProperty);
            }
            set
            {
                SetValue(TitleBarVisibilityProperty, value);
            }
        }
        #endregion

        /// <summary>
        /// close window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void titleBar_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult ret = MessageBox.Show(LanguageManager.Instance["CloseAppConfirmation"], LanguageManager.Instance["Tip"], MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ret == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Window window = sender as Window;
            if (window.WindowState == WindowState.Maximized)
            {
                this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                titleBar.MaximizeButtonToolTip = LanguageManager.Instance["RestoreBtnName"];
            }
            else
            {
                titleBar.MaximizeButtonToolTip = LanguageManager.Instance["MaximizeBtnName"];
            }
        }

        private void providedBy_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            String url = "https://github.com/BugLordI";
            System.Diagnostics.Process.Start("explorer.exe", url);
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            if (users.Count == 0)
            {
                MessageBox.Show(LanguageManager.Instance["ImportStaffConfigTip"],
                  LanguageManager.Instance["Tip"], MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (awardConfigurations.Count == 1)
            {
                MessageBoxResult ret = MessageBox.Show(LanguageManager.Instance["LotteryOverTip"],
               LanguageManager.Instance["Tip"], MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ret == MessageBoxResult.Yes)
                {
                    loadUser();
                    awardConfigurations = null;
                    loadAwardConfiguration(0);
                    winnerList.Children.Clear();
                }
            }
            else
            {
                timer.Start();
                StartIsEnable = false;
                StopIsEnable = true;
                if (currentShow.Number > currentCount)
                {
                    AwardConfigurationTip = String.Format(LanguageManager.Instance["AwardConfigurationTip"]
                      , currentShow.Label, ++currentCount, currentShow.Number);
                }
                else
                {
                    winnerList.Children.Clear();
                    loadAwardConfiguration(1);
                }
            }
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopIsEnable = false;
            StartIsEnable = true;
            timer.Stop();
            TextBlock textBlock = new TextBlock();
            textBlock.FontSize= 24;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Text = userTb.Text;
            textBlock.Margin = new Thickness(0,2,0,0);
            textBlock.Foreground = WelcomeForeground;
            userTb.Text = WelcomeStr;
            winnerList.Children.Add(textBlock);
            if (!repeatParticipation)
            {
                // 去掉已经中奖的
                users = users.Where(e => !$"{e.Name}({e.No})".Equals(textBlock.Text)).ToList();
            }
            if (currentShow.Number == currentCount)
            {
                exportWinnerList(currentShow.Label);
            }
        }

        private void exportWinnerList(String header)
        {
            StringBuilder sb = new StringBuilder(header);
            sb.Append(LanguageManager.Instance["WinnerListTitle"]).Append(Environment.NewLine);
            foreach (var item in winnerList.Children)
            {
                TextBlock textBlock = item as TextBlock;
                sb.AppendLine(textBlock.Text);
            }
            using StreamWriter writer = new StreamWriter(winnerListFile, true);
            writer.Write(sb.ToString());
        }
    }
}
