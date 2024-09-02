using LiteDB;
using Prism.Ioc;
using Prism.Mvvm;
using System.Collections.Generic;

namespace InitializeDatabase.ViewModels
{
    public class SubSystemInfoViewModel : BindableBase
    {
        private readonly ILiteDatabase rawDataDb;

        public SubSystemInfoViewModel()
        {
            rawDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("RawData");
            SubSystems = rawDataDb.GetCollection<SubSystem>("subsystem").Query().ToList();

            //var result = _dAFacade.Query<SubSystem>("SELECT PKEY,NAME,IS_PHYSICAL,EXCLUSIVE_CONTROL,CREATED_BY,DATE_CREATED,MODIFIED_BY,DATE_MODIFIED,DISPLAY_NAME,LOC_EXCLUSIVE_CONTROL,ORDERID,PARENT_KEY FROM SUBSYSTEM");
            //var col = rawDataDb.GetCollection<SubSystem>("subsystem");
            //col.InsertBulk(result);
        }

        private List<SubSystem> _SubSystems;

        public List<SubSystem> SubSystems
        {
            get { return _SubSystems; }
            set { SetProperty(ref _SubSystems, value); }
        }
    }
}