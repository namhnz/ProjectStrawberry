using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MacSetter.Essential
{
    public class TabVisibility : DependencyObject
    {
        public static readonly DependencyProperty LeftTabVisibilityProperty
            = DependencyProperty.Register("LeftTabVisibility", typeof(bool), typeof(TabVisibility), new UIPropertyMetadata(true));
        public bool LefTabVisibility
        {
            get { return (bool)GetValue(LeftTabVisibilityProperty); }
            set { SetValue(LeftTabVisibilityProperty, value); }
        }
    }
}
