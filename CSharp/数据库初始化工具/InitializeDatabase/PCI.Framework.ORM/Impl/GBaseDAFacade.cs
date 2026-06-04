using System.Data;
using System.Data.Odbc;

namespace PCI.Framework.ORM.Impl
{
    internal class GBaseDAFacade : DAFacade
    {
        public override string Prefix => ":";

        public override ConnectType DBType => ConnectType.GBase;

        public GBaseDAFacade(string constr)
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.GBaseDialect();
            CreateConnection(constr);
        }

        public override IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            OdbcDataAdapter adapter = new OdbcDataAdapter((OdbcCommand)cmd);
            OdbcCommandBuilder builder = new OdbcCommandBuilder(adapter);
            return adapter;
        }

        public override IDbDataParameter GetParameter(string name, object value, DbType type = DbType.String, int size = 100, ParameterDirection direction = ParameterDirection.Input)
        {
            OdbcParameter parameter = new OdbcParameter
            {
                DbType = type,
                ParameterName = name,
                Value = value,
                Direction = direction,
                Size = size
            };
            return parameter;
        }

        protected override void CreateConnection(string constr)
        {
            this.Connection = new OdbcConnection(constr);
        }

        /// <summary>
        /// 分页查询，但暂不清楚限制，姑且使用Oracle兼容
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="page"></param>
        /// <param name="resultsPerPage"></param>
        /// <returns></returns>
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
