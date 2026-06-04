using CommunityToolkit.Mvvm.ComponentModel;

namespace WPFDeveloper.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        // 这个类继承自CommunityToolkit.Mvvm的ObservableObject，
        // 它已经实现了INotifyPropertyChanged接口
        // 我们可以在这里添加所有ViewModel共享的功能
    }
}