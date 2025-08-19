using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace InitializeDatabase.Helper
{
    public   class MoveRowToFirstBehavior:Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var checkbox = sender as DataGridCell;
            if (checkbox == null) return;
            var checkboxContent= checkbox.Content as CheckBox;
            if (checkboxContent == null) return;
            checkboxContent.Loaded += CheckboxContent_Loaded; ;
        }

        private void CheckboxContent_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox == null) return;

            var dataContext = checkbox.DataContext;
            //var dataGrid = this

        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.Loaded -= AssociatedObject_Loaded;
            }
        }
    }
}
