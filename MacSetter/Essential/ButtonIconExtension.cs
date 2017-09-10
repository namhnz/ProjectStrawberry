using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MacSetter.Essential
{
    public class ButtonIconExtension
    {
        public static readonly DependencyProperty IconInsideProperty =
            DependencyProperty.RegisterAttached("IconInside", typeof(string), typeof(ButtonIconExtension), new PropertyMetadata(default(string)));

        public static void SetIconInside(UIElement element, string value)
        {
            element.SetValue(IconInsideProperty, value);
        }

        public static string GetIconInside(UIElement element)
        {
            return (string)element.GetValue(IconInsideProperty);
        }
    }
}
