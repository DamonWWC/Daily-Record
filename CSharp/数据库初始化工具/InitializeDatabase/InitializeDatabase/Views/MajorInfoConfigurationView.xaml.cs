using InitializeDatabase.ViewModels;
using System.Windows.Controls;

namespace InitializeDatabase.Views
{
    /// <summary>
    /// Interaction logic for MajorInfoConfigurationView
    /// </summary>
    public partial class MajorInfoConfigurationView : UserControl
    {
        public MajorInfoConfigurationView()
        {
            InitializeComponent();
            if (DataContext is MajorInfoConfigurationViewModel viewModel)
            {
                foreach (var item in viewModel.Columns)
                {
                    dg.Columns.Add(item);
                }
            }
        }
    }
}