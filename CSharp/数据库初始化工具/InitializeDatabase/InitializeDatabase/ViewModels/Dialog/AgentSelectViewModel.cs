using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InitializeDatabase.ViewModels.Dialog
{
    public class AgentSelectViewModel : BindableBase, IDialogAware
    {
        public AgentSelectViewModel()
        {
        }

        public string Title => "Agent选择";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        private List<AgentInfo> _ConfigInfos;

        public List<AgentInfo> ConfigInfos
        {
            get { return _ConfigInfos; }
            set { SetProperty(ref _ConfigInfos, value); }
        }

        private DelegateCommand _ConfirmCommand;
        public DelegateCommand ConfirmCommand => _ConfirmCommand ??= new DelegateCommand(ExecuteConfirmCommand);

        private void ExecuteConfirmCommand()
        {     
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK,new DialogParameters { { "info",_addNewItems} }));
        }

        private DelegateCommand _CancelCommand;
        public DelegateCommand CancelCommand => _CancelCommand ??= new DelegateCommand(ExecuteCancelCommand);

        private void ExecuteCancelCommand()
        {
            _addNewItems.ForEach(item => item.IsChecked = !item.IsChecked);
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }
       
        private readonly List<AgentInfo> _addNewItems = [];
        private DelegateCommand<object> _OperCommand;

        public DelegateCommand<object> OperCommand => _OperCommand ??= new DelegateCommand<object>(ExecuteOperCommand);

        private void ExecuteOperCommand(object param)
        {
            if (param is AgentInfo info)
            {              
                info.IsChecked = !info.IsChecked;
                if (!_addNewItems.Any(p => p.Id == info.Id))
                {
                    _addNewItems.Add(info);
                }
                else
                {
                    _addNewItems.Remove(info);
                }
            }
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("info", out List<AgentInfo> configInfos))
            {
                ConfigInfos = configInfos;
            }
        }
    }
}