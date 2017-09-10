using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MacSetter.SettingsWindowResources.More
{
    public class SelectedButtonIndex : DependencyObject
    {
        private static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SelectedButtonIndex), new PropertyMetadata(default(int)));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static void SetSelectedIndex(UIElement element, int value)
        {
            element.SetValue(SelectedIndexProperty, value);
        }

        public static int GetSelectedIndex(UIElement element)
        {
            return (int)element.GetValue(SelectedIndexProperty);
        }
    }
}
