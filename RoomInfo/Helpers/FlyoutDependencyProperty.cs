using Microsoft.Xaml.Interactivity;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace RoomInfo.Helpers
{
    public class FlyoutDependencyProperty : DependencyObject, IBehavior
    {
        public FlyoutDependencyProperty()
        {
            OpenActions = new ActionCollection();
            CloseActions = new ActionCollection();
        }
        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            var flyoutBase = associatedObject as FlyoutBase;
            if (flyoutBase == null) throw new ArgumentException("FlyoutBehavior can only be attached to FlyoutBase");
            AssociatedObject = associatedObject;
            flyoutBase.Opened += (s, e) =>
            {
                foreach (IAction action in OpenActions)
                {
                    action.Execute(AssociatedObject, null);
                }
            };
            flyoutBase.Closed += (s, e) =>
            {
                foreach (IAction action in CloseActions)
                {
                    action.Execute(AssociatedObject, null);
                }
            };
        }

        public void Detach()
        {
            AssociatedObject = null;
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenActionsProperty =
            DependencyProperty.Register("OpenActions", typeof(ActionCollection), typeof(FlyoutDependencyProperty), new PropertyMetadata(null));

        public static readonly DependencyProperty CloseActionsProperty =
            DependencyProperty.Register("CloseActions", typeof(ActionCollection), typeof(FlyoutDependencyProperty), new PropertyMetadata(null));

        public ActionCollection OpenActions
        {
            get { return GetValue(OpenActionsProperty) as ActionCollection; }
            set { SetValue(OpenActionsProperty, value); }
        }
        public ActionCollection CloseActions
        {
            get { return GetValue(CloseActionsProperty) as ActionCollection; }
            set { SetValue(CloseActionsProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached(
            "IsOpen",
            typeof(bool),
            typeof(FlyoutDependencyProperty),
            new PropertyMetadata(true, IsOpenChangedCallback));

        public static readonly DependencyProperty ParentProperty = DependencyProperty.RegisterAttached(
            "Parent",
            typeof(FrameworkElement),
            typeof(FlyoutDependencyProperty), null);

        public static void SetIsOpen(DependencyObject element, bool value)
        {
            element.SetValue(IsVisibleProperty, value);
        }

        public static bool GetIsOpen(DependencyObject element)
        {
            return (bool)element.GetValue(IsVisibleProperty);
        }

        private static void IsOpenChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var flyoutBase = d as FlyoutBase;
            if (flyoutBase == null)
                return;

            if ((bool)e.NewValue)
            {
                flyoutBase.Closed += flyout_Closed;
                var parent = GetParent(d);
                if (parent != null) flyoutBase.ShowAt(parent);
            }
            else
            {
                flyoutBase.Closed -= flyout_Closed;
                flyoutBase.Hide();
            }
        }

        private static void flyout_Closed(object sender, object e)
        {
            // When the flyout is closed, sets its IsOpen attached property to false.
            SetIsOpen(sender as DependencyObject, false);
        }

        public static void SetParent(DependencyObject element, FrameworkElement value)
        {
            element.SetValue(ParentProperty, value);
        }

        public static FrameworkElement GetParent(DependencyObject element)
        {
            return (FrameworkElement)element.GetValue(ParentProperty);
        }
    }

    public class OpenFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            return null;
        }
    }
}
