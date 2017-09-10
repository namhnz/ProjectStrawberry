using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MacSetter.Essential
{
    public class ButtonChangeState : DependencyObject
    {
        private static readonly DependencyProperty ButtonChangingStateProperty =
            DependencyProperty.Register("ButtonChangingState", typeof(ChangingState), typeof(ButtonChangeState), new PropertyMetadata(ChangingState.Default));

        public ChangingState ButtonChangingState
        {
            get { return (ChangingState)GetValue(ButtonChangingStateProperty); }
            set { SetValue(ButtonChangingStateProperty, value); }
        }

        public static void SetButtonChangingState(UIElement element, string value)
        {
            element.SetValue(ButtonChangingStateProperty, value);
        }

        public static ChangingState GetButtonChangingState(UIElement element)
        {
            return (ChangingState)element.GetValue(ButtonChangingStateProperty);
        }
    }
    public enum ChangingState
    {
        Default,
        Changing,
        Success,
        Failure,
    }
}
