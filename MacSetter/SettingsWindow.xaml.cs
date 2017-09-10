using MacSetter.SettingsWindowResources.More;
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

namespace MacSetter
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private SelectedButtonIndex selectedIndex = new SelectedButtonIndex();

        public SelectedButtonIndex SelectedIndex { get => selectedIndex; set => selectedIndex = value; }

        private void Button_Click_0(object sender, RoutedEventArgs e)
        {
            selectedIndex.SelectedIndex = 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            selectedIndex.SelectedIndex = 1;
        }
    }
}
