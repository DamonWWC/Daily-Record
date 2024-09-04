using System;

namespace InitializeDatabase
{
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