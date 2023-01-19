using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RadarGame.UI.Controls
{
    [TemplateVisualState(Name = "IsGettingData", GroupName = "Selection")]
    [TemplateVisualState(Name = "IsTurnedOn", GroupName = "Selection")]
    [TemplateVisualState(Name = "IsTurnedOff", GroupName = "Selection")]

    public class PowerButton : Button
    {
        public static readonly DependencyProperty IsGettingDataProperty = DependencyProperty.Register(
            "IsGettingData", typeof(bool), typeof(PowerButton), new PropertyMetadata(default(bool), isGettingDataCallBack));

        public bool IsGettingData
        {
            get { return (bool)GetValue(IsGettingDataProperty); }
            set { SetValue(IsGettingDataProperty, value); }
        }

        public static readonly DependencyProperty IsTurnedOnProperty = DependencyProperty.Register(
            "IsTurnedOn", typeof(bool), typeof(PowerButton), new PropertyMetadata(default(bool), isTurnedOnCallBack));

        public bool IsTurnedOn
        {
            get { return (bool)GetValue(IsTurnedOnProperty); }
            set { SetValue(IsTurnedOnProperty, value); }
        }

        
        private static void isGettingDataCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PowerButton;
            if ((bool)e.NewValue)
            {
                VisualStateManager.GoToState(control, "IsGettingData", true);
            }

            if ((bool)e.NewValue == false && control.IsTurnedOn == false)
            {
                VisualStateManager.GoToState(control, "IsTurnedOff", true);
            }
        }
        private static void isTurnedOnCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PowerButton;
            if ((bool)e.NewValue)
            {
                VisualStateManager.GoToState(control, "IsTurnedOn", true);
            }
            else
            {
                VisualStateManager.GoToState(control, "IsTurnedOff", true);
            }
        }
        public PowerButton()
        {
            DefaultStyleKey = typeof(PowerButton);
        }
    }
}
