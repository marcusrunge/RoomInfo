using Microsoft.Xaml.Interactivity;

using Prism.Windows.Navigation;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RoomInfo.Behaviors
{
    public class PivotNavigationBehavior : Behavior<Pivot>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PivotItemUnloading += AssociatedObject_PivotItemUnloading;
            AssociatedObject.PivotItemLoading += AssociatedObject_PivotItemLoading;
        }

        private void AssociatedObject_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            var navAwarePivot = ((args.Item.Content as Frame).Content as FrameworkElement).DataContext as INavigationAware;
            navAwarePivot?.OnNavigatedTo(null, null);
        }

        private void AssociatedObject_PivotItemUnloading(Pivot sender, PivotItemEventArgs args)
        {
            var navAwarePivot = ((args.Item.Content as Frame).Content as FrameworkElement).DataContext as INavigationAware;
            navAwarePivot?.OnNavigatingFrom(null, null, false);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PivotItemUnloading -= AssociatedObject_PivotItemUnloading;
            AssociatedObject.PivotItemLoading -= AssociatedObject_PivotItemLoading;
        }
    }
}