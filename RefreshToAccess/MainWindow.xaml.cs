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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RefreshToAccess
{
    public partial class MainWindow : Window
    {
        int tokenType = 0;
        public static int riseLauncherCount = 0;
        public static string profileName = "Waiting for login...";
        public static string playerUuid = "Waiting for login...";
        public static string accessToken = "";
        public MainWindow()
        {
            InitializeComponent();
            tokenIsLunar.IsChecked=true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RefreshBox.Text==""||RefreshBox.Text=="Your RefreshToken here...")
                {
                    throw new Exception("You didn't input your refresh token");
                }
                Indicator.Content="Logging in...";
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
                Indicator.Content="Login successful";
                accessToken=token;
                AccessBox.Text=token;
                Clipboard.SetText(AccessBox.Text);
                MessageBox.Show("Successfully logged in\nPlayer profile name："+username+"\n"+"Player uuid: "+uuid+"\n\n"+"AccessToken copied to your clipboard");
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
                Indicator.Content="Error";
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AccessBox.Text);
        }

        private void RefreshBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (RefreshBox.Text=="Your RefreshToken here...")
            {
                RefreshBox.Text="";
            }
        }
        private void RefreshBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RefreshBox.Text=="")
            {
                RefreshBox.Text="Your RefreshToken here...";
            }
        }
        private void AccessBox_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (AccessBox.Text=="Your AccessToken will be here...")
            {
                AccessBox.Text="";
            }
        }
        private void AccessBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (AccessBox.Text=="")
            {
                AccessBox.Text="Your AccessToken will be here...";
            }
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
    }
}
