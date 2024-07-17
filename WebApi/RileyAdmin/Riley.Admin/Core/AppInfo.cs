using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Riley.Admin.Core
{
    public static class AppInfo
    {
        static AppInfo()
        {

        }
        private static bool _isRun;
        public static bool IsRun
        {
            get => _isRun;
            set => _isRun = value;
        }

    }
}
