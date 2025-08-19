using PCI.Framework.ORM.DapperExtensions.LambdaExtension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PCI.Framework.ORM.Impl
{
    /// <summary>
    /// 实现连接SQLServer数据库
    /// </summary>
    internal class SQLServerDAFacade : DAFacade
    {
        public SQLServerDAFacade(string constr)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.SqlServerDialect();
            CreateConnection(constr);
        }

        public override string Prefix
        {
            get { return "@"; }
        }
        public override ConnectType DBType
        {
            get { return ConnectType.SQLServer; }
        }

        protected override void CreateConnection(string constr)
        {
            this.Connection = new SqlConnection(constr);
        }

        public override IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            SqlDataAdapter adapter = new SqlDataAdapter((SqlCommand)cmd);
            SqlCommandBuilder cb = new SqlCommandBuilder(adapter);
            return adapter;
        }

        public override IDbDataParameter GetParameter(string name, object value, DbType type, int size = 100, ParameterDirection direction = ParameterDirection.Input)
        {
            SqlParameter parameter = new SqlParameter
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
            //未实现
            return sql;
        }
    }
}
