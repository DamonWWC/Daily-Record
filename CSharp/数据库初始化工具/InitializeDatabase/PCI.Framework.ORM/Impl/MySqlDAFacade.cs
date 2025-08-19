using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using PCI.Framework.ORM.DapperExtensions.LambdaExtension;

namespace PCI.Framework.ORM.Impl
{
    /// <summary>
    /// 实现连接Mysql数据库
    /// </summary>
    internal class MySqlDAFacade : DAFacade
    {
        public MySqlDAFacade(string constr)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.MySqlDialect();
            CreateConnection(constr);
        }

        public override string Prefix
        {
            get { return "@"; }
        }
        public override ConnectType DBType
        {
            get { return ConnectType.MySql; }
        }

        protected override void CreateConnection(string constr)
        {
            this.Connection = new MySqlConnection(constr);
        }

        public override IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter((MySqlCommand)cmd);
            MySqlCommandBuilder cb = new MySqlCommandBuilder(adapter);
            return adapter;
        }

        public override IDbDataParameter GetParameter(string name, object value, DbType type, int size = 100, ParameterDirection direction = ParameterDirection.Input)
        {
            MySqlParameter parameter = new MySqlParameter
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
            string pageSql = string.Format("{0} limit {1},{2}", sql, resultsPerPage * (page - 1), resultsPerPage);
            return pageSql;
        }
    }
}
