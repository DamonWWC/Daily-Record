using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dm;
using PCI.Framework.ORM.DapperExtensions.LambdaExtension;

namespace PCI.Framework.ORM.Impl
{
    /// <summary>
    /// 实现连接达梦数据库
    /// </summary>
    internal class DmDAFacade : DAFacade
    {
        public DmDAFacade(string constr)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.DmDialect();
            CreateConnection(constr);
        }

        public override string Prefix
        {
            get { return ":"; }
        }

        public override ConnectType DBType
        {
            get { return ConnectType.Dm; }
        }

        protected override void CreateConnection(string constr)
        {
            this.Connection = new DmConnection(constr);
        }

        public override IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            DmDataAdapter adapter = new DmDataAdapter((DmCommand)cmd);
            DmCommandBuilder cb = new DmCommandBuilder(adapter);
            return adapter;
        }

        public override IDbDataParameter GetParameter(string name, object value, DbType type, int size = 100, ParameterDirection direction = ParameterDirection.Input)
        {
            DmParameter parameter = new DmParameter
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
            string pageSql = string.Format("SELECT * FROM(SELECT tt.*, ROWNUM AS rowno " +
               " FROM( {0} ) tt " +
               " WHERE ROWNUM <= {2}) table_alias  " +
               " WHERE table_alias.rowno >= {1}  ", sql, 1 + resultsPerPage * (page - 1), resultsPerPage * page);
            return pageSql;
        }
    }
}
