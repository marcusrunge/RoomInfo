using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RoomInfo.Helpers
{
    public class VisualStateProperty : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            if (associatedObject is Control control) AssociatedObject = associatedObject;
        }

        public void Detach()
        {
            AssociatedObject = null;
        }

        public object VisualState
        {
            get { return (string)GetValue(VisualStatePropertyProperty); }
            set { SetValue(VisualStatePropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisualStatePropertyProperty =
            DependencyProperty.Register("VisualStateProperty", typeof(int), typeof(VisualStateProperty), new PropertyMetadata(null, VisualStatePropertyChanged));

        private static void VisualStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var visualStateProperty = d as VisualStateProperty;
            if (visualStateProperty.AssociatedObject == null || e.NewValue == null) return;
            VisualStateManager.GoToState(visualStateProperty.AssociatedObject as Control, e.NewValue.ToString(), true);
        }
    }
}
