using LiteDB;
using PCI.Framework.ORM;
using PCI.Framework.ORM.Dapper;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace InitializeDatabase.ViewModels
{
	public class SubSystemConfigurationViewModel : BindableBase
	{
        readonly IDAFacade _dAFacade;
        private readonly ILiteDatabase initDataDb;
        private readonly ILiteDatabase rawDataDb;

        
        public SubSystemConfigurationViewModel( IDAFacade dAFacade)
        {
            _dAFacade = dAFacade;
            initDataDb= ContainerLocator.Container.Resolve<ILiteDatabase>("InitData");
            rawDataDb= ContainerLocator.Container.Resolve<ILiteDatabase>("RawData");
         
            //var result = _dAFacade.Query<SubSystem>("SELECT PKEY,NAME,IS_PHYSICAL,EXCLUSIVE_CONTROL,CREATED_BY,DATE_CREATED,MODIFIED_BY,DATE_MODIFIED,DISPLAY_NAME,LOC_EXCLUSIVE_CONTROL,ORDERID,PARENT_KEY FROM SUBSYSTEM");
            //var col = rawDataDb.GetCollection<SubSystem>("subsystem");
            //col.InsertBulk(result);
        }


        private ObservableCollection<MajorInfo> _MajorInfos;
        public ObservableCollection<MajorInfo> MajorInfos
        {
            get { return _MajorInfos; }
            set { SetProperty(ref _MajorInfos, value); }
        }


        #region cmd
        private DelegateCommand _PasteCommand;
        public DelegateCommand PasteCommand => _PasteCommand ??= new DelegateCommand(ExecutePasteCommand);

        void ExecutePasteCommand()
        {
            var pasteinfo = Clipboard.GetText();           
            List< MajorInfo> majorInfos = [];
            var col = rawDataDb.GetCollection<SubSystem>("subsystem");

            var cells = pasteinfo.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string item in cells)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                var items = item.Split('\t');
                var subname = items.ElementAtOrDefault(1);
                
                
                majorInfos.Add(new MajorInfo
                {
                    Name = items.ElementAtOrDefault(0),
                    SubName = items.ElementAtOrDefault(1),
                    Agent=items.ElementAtOrDefault(2),
                    SubSystemKey=col.FindOne(p=>p.NAME== subname)?.PKEY
                });
            }
            
         
            MajorInfos = new ObservableCollection<MajorInfo>(majorInfos);
            
        }




        #endregion
    }


    public class MajorInfo
    {
        public int? SubSystemKey { get; set; }
        public string Name { get; set; }
        public string SubName { get; set; }
        public string Agent { get; set; }
    }

    public class SubSystem
    {
        public int PKEY { get; set; }
        public string NAME { get; set; }
        public int IS_PHYSICAL { get; set; }
        public int EXCLUSIVE_CONTROL { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime DATE_CREATED { get; set; }
        public string MODIFIED_BY { get; set; }

        public DateTime DATE_MODIFIED { get; set; }
        public string DISPLAY_NAME { get; set; }
        public int LOC_EXCLUSIVE_CONTROL { get; set; }
        public int ORDERID { get; set; }
        public int PARENT_KEY { get; set; }

    }
}
