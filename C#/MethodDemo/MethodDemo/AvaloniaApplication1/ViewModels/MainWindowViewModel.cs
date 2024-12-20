using ReactiveUI;
using System.Windows.Input;

namespace AvaloniaApplication1.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting { get; } = "Welcome to Avalonia!";


        public MainWindowViewModel()
        {
                OpenThePodBayDoorsDirectCommand=ReactiveCommand.Create(OpenThePod)
        }
        public ICommand OpenThePodBayDoorsDirectCommand { get; }
    }
}
