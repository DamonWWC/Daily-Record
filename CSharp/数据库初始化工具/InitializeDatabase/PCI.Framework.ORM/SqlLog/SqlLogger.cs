using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Framework.ORM.SqlLog
{
    /// <summary>
    /// 日志记录入口类
    /// </summary>
    public class SqlLogger
    {
        /// <summary>
        /// 通过推送日志信息，让客户端去记录日志
        /// </summary>
        public static Action<SqlLogInfo> Log;

        /// <summary>
        /// 异步触发Log委托的执行
        /// </summary>
        /// <param name="info"></param>
        internal static void OnLog(SqlLogInfo info)
        {
            Task.Factory.StartNew(x =>
            {
                try
                {
                    Log?.Invoke(x as SqlLogInfo);
                }
                catch (Exception e)
                { 
                    //防止客户端日志记录日志异常影响PCI.Framework.ORM的SQL语句执行
                }
            }, info);
        }
    }

    /// <summary>
    /// Sql日志信息
    /// </summary>
    public class SqlLogInfo
    {
        private System.Data.IDbCommand command;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="command"></param>
        public SqlLogInfo(System.Data.IDbCommand command)
        {
            this.command = command;
        }
        /// <summary>
        /// SQL语句
        /// </summary>
        public string Sql
        {
            get { return this.command?.CommandText; }
        }

        /// <summary>
        /// SQL语句执行时的参数
        /// </summary>
        public Dictionary<string, object> Parameters
        {
            get
            {
                if (this.command == null) return new Dictionary<string, object>();
                if (this.command.Parameters == null || this.command.Parameters.Count == 0) return new Dictionary<string, object>();

                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach (var p in this.command.Parameters)
                {
                    var dbParam = p as System.Data.Common.DbParameter;
                    if (dbParam != null)
                    {
                        dic.Add(dbParam.ParameterName, dbParam.Value);
                    }
                }
                return dic;
            }
        }
    }
}
