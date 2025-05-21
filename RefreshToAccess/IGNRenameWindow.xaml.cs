using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RefreshToAccess
{
    /// <summary>
    /// IGNRenameWindow.xaml 的交互逻辑
    /// </summary>
    public partial class IGNRenameWindow : Window
    {
        public IGNRenameWindow()
        {
            InitializeComponent();
            IGNBox.Text=MainWindow.profileName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(IGNBox.Text==MainWindow.profileName)
            {
                MessageBox.Show("The profile name hasn't changed, try changing it to something else.");
            }
            else
            {
                IGNRename.Rename(IGNBox.Text);
            }
        }
    }
}
