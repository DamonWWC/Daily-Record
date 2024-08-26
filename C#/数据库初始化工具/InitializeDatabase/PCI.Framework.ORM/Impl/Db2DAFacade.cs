using IBM.Data.DB2;
using PCI.Framework.ORM.DapperExtensions.LambdaExtension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PCI.Framework.ORM.Impl
{
    /// <summary>
    /// 实现连接DB2数据库
    /// </summary>
    internal class Db2DAFacade : DAFacade
    {
        public Db2DAFacade(string constr)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.DB2Dialect();
            CreateConnection(constr);
        }

        public override string Prefix
        {
            get { return "@"; }
        }

        public override ConnectType DBType
        {
            get { return ConnectType.Db2; }
        }

        protected override void CreateConnection(string constr)
        {
            this.Connection = new DB2Connection(constr);
        }

        public override IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            DB2DataAdapter adapter = new DB2DataAdapter((DB2Command)cmd);
            DB2CommandBuilder cb = new DB2CommandBuilder(adapter);

            return adapter;
        }

        public override IDbDataParameter GetParameter(string name, object value, DbType type, int size=100, ParameterDirection direction= ParameterDirection.Input)
        {
            DB2Parameter parameter = new DB2Parameter
            {
                DbType = type,
                ParameterName = name,
                Value = value,
                Direction = ParameterDirection.Input,
                Size = size
            };
            return parameter;
        }

        protected override string GetPageSql(string sql, int page, int resultsPerPage)
        {
            string pageSql = string.Format("select * from (" +
                "select ROW_NUMBER() OVER () AS ROWNUM,t.* from ({0}) t ) where ROWNUM BETWEEN {1} AND {2}", sql, 1 + resultsPerPage * (page - 1), resultsPerPage * page);
            return pageSql;
        }
    }
}
