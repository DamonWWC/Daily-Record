using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using PCI.Framework.ORM.DapperExtensions.LambdaExtension;

namespace PCI.Framework.ORM.Impl
{
    /// <summary>
    /// 实现连接Oracle数据库
    /// </summary>
    internal class OracleDAFacade : DAFacade
    {
        public OracleDAFacade(string constr)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.OracleDialect();
            CreateConnection(constr);
        }

        public override string Prefix
        {
            get { return ":"; }
        }

        public override ConnectType DBType
        {
            get { return ConnectType.Oracle; }
        }

        protected override void CreateConnection(string constr)
        {
            this.Connection = new OracleConnection(constr);
        }

        public override IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            OracleDataAdapter adapter = new OracleDataAdapter((OracleCommand)cmd);
            OracleCommandBuilder cb = new OracleCommandBuilder(adapter);
            return adapter;
        }

        public override IDbDataParameter GetParameter(string name, object value, DbType type, int size = 100, ParameterDirection direction = ParameterDirection.Input)
        {
            OracleParameter parameter = new OracleParameter
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
