using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MacSetter.Essential
{
    class ButtonTextExtension
    {
        public static readonly DependencyProperty TextInsideProperty =
            DependencyProperty.RegisterAttached("TextInside", typeof(string), typeof(ButtonTextExtension), new PropertyMetadata(default(string)));

        public static void SetTextInside(UIElement element, string value)
        {
            element.SetValue(TextInsideProperty, value);
        }

        public static string GetTextInside(UIElement element)
        {
            return (string)element.GetValue(TextInsideProperty);
        }
    }
}
