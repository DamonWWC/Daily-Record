using Prism.Mvvm;

namespace DataTemplateDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {
            TemplateData = new Class1();
        }
        private object _TemplateData;
        public object TemplateData
        {
            get { return _TemplateData; }
            set { SetProperty(ref _TemplateData, value); }
        }
    }
}
