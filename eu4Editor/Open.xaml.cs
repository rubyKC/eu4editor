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
using Microsoft.Win32;

namespace Eu4Editor
{
    /// <summary>
    /// Open.xaml 的交互逻辑
    /// </summary>
    public partial class Open : Window
    {
        public Open()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void ButtonBase_OnClick1(object sender, RoutedEventArgs e)
        {


        }
    }
}
