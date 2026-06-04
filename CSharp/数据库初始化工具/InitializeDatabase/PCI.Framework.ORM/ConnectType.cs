using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCI.Framework.ORM
{
    /// <summary>
    /// 数据库连接类型
    /// </summary>
    public enum ConnectType
    {
        /// <summary>
        /// sysbase数据库连接
        /// </summary>
        Sybase,
        /// <summary>
        /// Sqlite数据库连接
        /// </summary>
        SQLite,
        /// <summary>
        /// SqlServer数据库连接
        /// </summary>
        SQLServer,
        /// <summary>
        /// Oracle数据库连接
        /// </summary>
        Oracle,
        /// <summary>
        /// MySql数据库连接
        /// </summary>
        MySql,
        /// <summary>
        /// DB2数据库连接
        /// </summary>
        Db2,
        /// <summary>
        /// PostgreSQL数据库连接
        /// </summary>
        PostgreSQL,
        /// <summary>
        /// 达梦数据库
        /// </summary>
        Dm,
        /// <summary>
        /// 南大通用数据库
        /// </summary>
        GBase
    }
}
