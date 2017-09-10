using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MacSetter.SettingsWindowResources.More
{
    public class IsElementSelectedExtension
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(IsElementSelectedExtension), new PropertyMetadata(default(bool)));

        public static void SetIsSelected(UIElement element, bool value)
        {
            element.SetValue(IsSelectedProperty, value);
        }

        public static bool GetIsSelected(UIElement element)
        {
            return (bool)element.GetValue(IsSelectedProperty);
        }
    }
}
