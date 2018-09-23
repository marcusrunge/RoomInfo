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
    public class VisualStateDependencyProperty : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            var control = associatedObject as Control;
            if (control == null) throw new ArgumentException("VisualStateDependencyProperty can be attached only to Control");

            AssociatedObject = associatedObject;
        }

        public void Detach()
        {
            AssociatedObject = null;
        }

        public object VisualStateProperty
        {
            get { return (Enum)GetValue(VisualStatePropertyProperty); }
            set { SetValue(VisualStatePropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisualStateProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisualStatePropertyProperty = DependencyProperty.Register(
            "VisualStateProperty",
            typeof(object),
            typeof(VisualStateDependencyProperty),
            new PropertyMetadata(null, VisualStatePropertyChanged));

        private static void VisualStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dependencyProperty = d as VisualStateDependencyProperty;
            if (dependencyProperty.AssociatedObject == null || e.NewValue == null) return;
            VisualStateManager.GoToState(dependencyProperty.AssociatedObject as Control, e.NewValue.ToString(), true);
        }
    }
}
