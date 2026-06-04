using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace InitializeDatabase.ViewModels.Dialog;

public class NewIceInfoViewModel : BindableBase, IDialogAware
{
    public NewIceInfoViewModel()
    {
    }

    private IceServiceBaseInfo _IceInfo=new IceServiceBaseInfo();

    public IceServiceBaseInfo IceInfo
    {
        get { return _IceInfo; }
        set { SetProperty(ref _IceInfo, value); }
    }


    private DelegateCommand _ConfirmCommand;
    public DelegateCommand ConfirmCommand => _ConfirmCommand ??= new DelegateCommand(ExecuteConfirmCommand);

    void ExecuteConfirmCommand()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters { { "info", IceInfo } }));
    }
    private DelegateCommand _CancelCommand;
    public DelegateCommand CancelCommand => _CancelCommand ??= new DelegateCommand(ExecuteCancelCommand);

    void ExecuteCancelCommand()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
    }

    public string Title { get; set; }

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
    }
}

