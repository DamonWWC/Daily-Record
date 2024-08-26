using PCI.Framework.ORM.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCI.Framework.ORM
{
    /// <summary>
    /// 数据访问门面工厂类
    /// 只用于创建不同数据库的数据访问门面，采用简单工厂模式。
    /// 
    /// 目的：
    ///     1.提供一个创建具体DAFacade实例的方法；
    ///     2.将创建具体的DAFacade实例和用户代码分离
    /// 
    /// 使用规范：
    ///     略
    /// </summary>
    public static class DAFacadeFactory
    {
        /// <summary>
        /// 创建连接器
        /// </summary>
        /// <param name="type">数据库类型</param>
        /// <param name="constr">数据库连接串</param>
        /// <param name="commandTimeout">执行超时时间</param>
        /// <returns>连接器</returns>
        public static IDAFacade CreateDAFacade(ConnectType type, string constr,int commandTimeout=30)
        {
            IDAFacade daFacade;
            switch (type)
            {
                case ConnectType.Sybase:
                    throw new NotImplementedException("未实现");
                //break;
                case ConnectType.SQLite:
                    throw new NotImplementedException("未实现");
                //break;
                case ConnectType.Oracle:
                    daFacade = new OracleDAFacade(constr) { CommanTimeout=commandTimeout};
                    break;
                case ConnectType.SQLServer:
                    daFacade = new SQLServerDAFacade(constr) { CommanTimeout = commandTimeout };
                    break;
                case ConnectType.MySql:
                    daFacade = new MySqlDAFacade(constr) { CommanTimeout = commandTimeout };
                    break;
                case ConnectType.Db2:
                    daFacade = new Db2DAFacade(constr) { CommanTimeout = commandTimeout };
                    break;
                case ConnectType.Dm:
                    daFacade = new DmDAFacade(constr) { CommanTimeout = commandTimeout };
                    break;
                case ConnectType.GBase:
                    daFacade = new GBaseDAFacade(constr) { CommanTimeout = commandTimeout };
                    break;
                default:
                    throw new Exception("数据库类型未指定");
            }
            return daFacade;
        }
    }
}
