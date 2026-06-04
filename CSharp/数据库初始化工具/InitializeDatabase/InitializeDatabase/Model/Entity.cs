using System;

namespace InitializeDatabase
{
    public class Entity
    {
        public int PKEY { get; set; }
        public string NAME { get; set; }
        public string ADDRESS { get; set; }
        public string DESCRIPTION { get; set; }
        public int SUBSYSTEMKEY { get; set; }
        public int LOCATIONKEY { get; set; }
        public int SEREGI_ID { get; set; }
        public int TYPEKEY { get; set; }
        public object PHYSICAL_SUBSYSTEM_KEY { get; set; }
        public object SCSDAT_ID { get; set; }
        public int PARENTKEY { get; set; }
        public int AGENTKEY { get; set; }
        public bool DELETED { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime DATE_CREATED { get; set; }
        public string MODIFIED_BY { get; set; }
        public DateTime DATE_MODIFIED { get; set; }
    }
}
