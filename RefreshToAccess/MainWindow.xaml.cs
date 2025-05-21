using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RefreshToAccess
{
    public partial class MainWindow : Window
    {
        int tokenType = 0;
        public static string profileName { get; set; } = "Waiting for login...";
        public static string playerUuid { get; set; } = "Waiting for login...";
        public static string accessToken { get; set; } = "";
        public static bool loggedIn = false;

        public MainWindow()
        {
            InitializeComponent();
            tokenIsLunar.IsChecked=true;
        }

        private void UpFloat_ShowLabel(string text)
        {
            Indicator.Content= text;
            Indicator.RenderTransform=new TranslateTransform(0, 10);
            Indicator.Opacity= 0;
            FrameworkElement notifyLabelElement = Indicator;
            TranslateTransform notifyLabelTransform = (TranslateTransform)Indicator.RenderTransform;
            DoubleAnimation animationTranslateFloatUp = new DoubleAnimation(0, TimeSpan.FromSeconds(0.4))
            {
                EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Power=5
                }
            };
            DoubleAnimation animationOpacityFull = new DoubleAnimation(1, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Power=2
                }
            };
            notifyLabelElement.BeginAnimation(Label.OpacityProperty, animationOpacityFull);
            notifyLabelTransform.BeginAnimation(TranslateTransform.YProperty, animationTranslateFloatUp);
        }
        private void UpFloat_HideLabel()
        {
            Indicator.RenderTransform=new TranslateTransform(0, 0);
            Indicator.Opacity= 1;
            FrameworkElement notifyLabelElement = Indicator;
            TranslateTransform notifyLabelTransform = (TranslateTransform)Indicator.RenderTransform;
            DoubleAnimation animationTranslateFloatUp = new DoubleAnimation(-10, TimeSpan.FromSeconds(0.4))
            {
                EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Power=5
                }
            };
            DoubleAnimation animationOpacityFull = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Power=2
                }
            };
            notifyLabelElement.BeginAnimation(Label.OpacityProperty, animationOpacityFull);
            notifyLabelTransform.BeginAnimation(TranslateTransform.YProperty, animationTranslateFloatUp);
        }

        public async void swapLabelText(string text)
        {
            UpFloat_HideLabel();
            await Task.Delay(300);
            UpFloat_ShowLabel(text);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            loggedIn=false;
            swapLabelText("Logging in...");
            try
            {
                if (RefreshBox.Text=="")
                {
                    throw new Exception("You didn't input your refresh token");
                }
                string[] AccProfile = { "null", "null", "null" };
                switch (tokenType)
                {
                    case 0:
                        AccProfile=await MSLogin.RequestTokenAsync(RefreshBox.Text, ClientIdentification.Vanilla);
                        break;
                    case 1:
                        AccProfile=await MSLogin.RequestTokenAsync(RefreshBox.Text, ClientIdentification.HMCL);
                        break;
                    case 2:
                        try
                        {
                            AccProfile=await MSLogin.RequestTokenAsync(RefreshBox.Text, ClientIdentification.essential);
                        }
                        catch (Exception)
                        {
                            AccProfile=await MSLogin.RequestTokenAsync(RefreshBox.Text, ClientIdentification.PCL);
                        }
                        break;
                }
                string username = AccProfile[0];
                string uuid = AccProfile[1];
                string token = AccProfile[2];
                profileName=username;
                IGNBox.Text=profileName;
                playerUuid=uuid;
                UUIDBox.Text=playerUuid;
                swapLabelText("Login successful");
                accessToken=token;
                AccessBox.Text=token;
                string Message = "Successfully logged in\nPlayer profile name："+username+"\n"+"Player uuid: "+uuid;
                if (AccTokenCopy.IsChecked==true)
                {
                    Clipboard.SetText(AccessBox.Text);
                    Message+="\n\n"+"AccessToken copied to your clipboard";
                }
                MessageBox.Show(Message, "Success");
                loggedIn=true;
                GC.Collect();
            }
            catch (Exception ex)
            {
                string ErrorMessage = "";
                if (ex.Message.Contains("400"))
                {
                    ErrorMessage="You chose the wrong token format, try again. If this error occours again, your token might be broken or expired, contact your alt seller.";
                }
                else if (ex.Message.Contains("429"))
                {
                    ErrorMessage="You're logging in too frequently, slow down. Try switching a VPN node or restart your network if this error keeps showing up.";
                }
                else if (ex.Message.Contains("502"))
                {
                    ErrorMessage="Your network is broken and failed to connect to microsoft service. Consider fixing it.";
                }
                if (ErrorMessage!="")
                {
                    MessageBox.Show("Something went wrong...\n\n"+ErrorMessage);
                }
                else
                {
                    MessageBox.Show("Something went wrong...\n\n"+ex.Message);
                }
                swapLabelText("Failed to login");
                GC.Collect();
            }
        }
        private void tokenIsLunar_Checked(object sender, RoutedEventArgs e)
        {
            tokenType= 0;
            tokenIsHmcl.IsChecked=false;
            tokenIsElse.IsChecked=false;
        }

        private void tokenIsHmcl_Checked(object sender, RoutedEventArgs e)
        {
            tokenType= 1;
            tokenIsLunar.IsChecked=false;
            tokenIsElse.IsChecked=false;
        }

        private void tokenIsElse_Checked(object sender, RoutedEventArgs e)
        {
            tokenType= 2;
            tokenIsLunar.IsChecked=false;
            tokenIsHmcl.IsChecked=false;
        }

        private void IGNBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(IGNBox.Text==profileName))
            {
                IGNBox.Text=profileName;
            }
        }

        private void UUIDBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(UUIDBox.Text==playerUuid))
            {
                UUIDBox.Text=playerUuid;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AccessBox.Text);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(profileName);
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(profileName);
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(playerUuid);
        }

        private void EditPfName(object sender, RoutedEventArgs e)
        {
            if (loggedIn)
            {
                IGNRenameWindow RenameWindow = new IGNRenameWindow();
                RenameWindow.Show();
            }
            else
            {
                MessageBox.Show("Make sure you have generated a proper Access Token, or else this won't work");
            }
        }
    }
}
