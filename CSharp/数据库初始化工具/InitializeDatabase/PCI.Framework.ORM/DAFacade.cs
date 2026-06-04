using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using PCI.Framework.ORM.Dapper;
using PCI.Framework.ORM.DapperExtensions;
using PCI.Framework.ORM.DapperExtensions.LambdaExtension;
using System.Linq.Expressions;
using PCI.Framework.ORM.DapperExtensions.Mapper;
namespace PCI.Framework.ORM
{
    /// <summary>
    /// 数据访问门面基类
    /// </summary>
    internal abstract class DAFacade : IDAFacade
    {
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public DAFacade()
        {
            SqlBuilder.DatabaseType = this.DBType;
            SqlBuilder.FuncGetTableName = GetTableName;
            SqlBuilder.FuncGetColumnName = GetColumnNameOnly;
        }
        #endregion


        private DbConnection connection;
        /// <summary>
        /// 数据库连接串
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                return connection;
            }
            protected set { connection = value; }
        }

        private int _commandTimeout=30;
        /// <summary>
        /// 查询超时时间
        /// </summary>
        public int CommanTimeout
        {
            get { return _commandTimeout; }
            set { _commandTimeout = value; }
        }

        #region 抽象、虚属性，方法
        /// <summary>
        /// 参数化sql的变量前缀
        /// </summary>
        public abstract string Prefix { get; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public abstract ConnectType DBType { get; }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        protected abstract void CreateConnection(string constr);

        /// <summary>
        /// 获取适配器
        /// </summary>
        /// <param name="cmd">数据库访问命令</param>
        /// <returns>适配器接口</returns>
        public abstract IDbDataAdapter GetAdapter(IDbCommand cmd);

        /// <summary>
        /// 创建命令
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">参数</param>
        /// <returns>sql命令</returns>
        public virtual IDbCommand GetCommand(string sql, IDataParameter[] para)
        {
            IDbCommand command = this.connection.CreateCommand();

            command.Parameters.Clear();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            if (para != null)
            {
                foreach (var p in para)
                {
                    command.Parameters.Add(p);
                }
            }
            return command;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="type">类型</param>
        /// <param name="size">参数长度</param>
        /// <param name="direction">参数方向</param>
        /// <returns>参数接口</returns>
        public abstract IDbDataParameter GetParameter(string name, object value, DbType type = DbType.String, int size = 100, ParameterDirection direction = ParameterDirection.Input);
        /// <summary>
        /// 获取分页语句
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="page">第几页</param>
        /// <param name="resultsPerPage">每页数量</param>
        /// <returns></returns>
        protected abstract string GetPageSql(string sql, int page, int resultsPerPage);
        /// <summary>
        /// 查询sql结果集的总数
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="param">参数</param>
        /// <returns>总数</returns>
        protected virtual int GetTotalCount(string sql, object param)
        {
            string str = string.Format("select count(*) from ({0}) a", sql);
            return this.connection.QueryFirst<int>(str, param);
        }

        #endregion
        /// <summary>
        /// 创建事务
        /// </summary>
        /// <returns></returns>
        public IDbTransaction CreateTransaction()
        {
            //打开连接
            if (this.connection.State == ConnectionState.Closed)
            {
                this.connection.Open();
            }
            return this.connection.BeginTransaction();
        }

        #region Dapper-Extensions
        /// <summary>
        /// 查询数量
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">过滤条件谓词</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>数量</returns>
        public virtual int Count<T>(object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Count<T>(predicate, transaction, commandTimeout);
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>主键</returns>
        public virtual object Insert<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Insert<T>(entity, transaction, commandTimeout);
        }
        /// <summary>
        /// 新增多个
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="entities">实体集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>主键</returns>
        public virtual void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            this.connection.Insert<T>(entities, transaction, commandTimeout);
        }
        /// <summary>
        /// 根据主键获取
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="id">主键</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>实体</returns>
        public virtual T Get<T>(object id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Get<T>(id, transaction, commandTimeout);
        }
        /// <summary>
        /// 获取全部
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">过滤条件谓词</param>
        /// <param name="sort">排序</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>结果集</returns>
        public virtual IEnumerable<T> GetList<T>(object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.GetList<T>(predicate, sort, transaction, commandTimeout);
        }
        /// <summary>
        /// 根据实体删除
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        public virtual bool Delete<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Delete<T>(entity, transaction, commandTimeout);
        }
        /// <summary>
        /// 根据谓词条件删除
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">谓词删除条件,使用IList IPredicate和IPredicateGroup</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        public virtual bool Delete<T>(object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Delete<T>(predicate, transaction, commandTimeout);
        }
        /// <summary>
        /// 根据sql传参删除
        /// </summary>
        /// <param name="sql">delete语句</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        public virtual bool Delete(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;

            // 使用参数化SQL语句GBase会报语法错误，现在此处将语句中参数替换为实际值
            if (DBType == ConnectType.GBase && param != null)
            {
                var paramNames = sql.Split(' ').Where(s => s.Contains("=:")).Select(s => s.Substring("=")).ToList();
                var paramType = param.GetType();
                var propNames = paramType.GetProperties();
                foreach (var name in paramNames)
                {
                    var expectPropName = name.Replace(":", "").ToLower();
                    var prop = propNames.Where(n => n.Name.ToLower() == expectPropName).FirstOrDefault();
                    if (prop != null)
                    {
                        if (prop.PropertyType == typeof(string))
                        {
                            sql = sql.Replace(name, $"'{prop.GetValue(param, null)}'");
                        }
                        else
                        {
                            sql = sql.Replace(name, prop.GetValue(param, null).ToString());
                        }
                    }
                }
                param = null;
            }

            return this.connection.Execute(sql, param, transaction, commandTimeout) > 0;
        }
        /// <summary>
        /// 根据实体更新
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="ignoreAllKeyProperties">不需更新的字段</param>
        /// <returns>是否成功</returns>
        public virtual bool Update<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Update<T>(entity, transaction, commandTimeout);
        }
        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="buffered">是否缓存</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>结果集</returns>
        public virtual IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;

            // 使用参数化SQL语句GBase会报语法错误，现在此处将语句中参数替换为实际值
            if (DBType == ConnectType.GBase && param != null)
            {
                var paramNames = sql.Split(' ').Where(s => s.Contains("=:")).Select(s => s.Substring("=")).ToList();
                var paramType = param.GetType();
                var propNames = paramType.GetProperties();
                foreach ( var name in paramNames)
                {
                    var expectPropName = name.Replace(":", "").ToLower();
                    var prop = propNames.Where(n => n.Name.ToLower() == expectPropName).FirstOrDefault();
                    if( prop != null )
                    {
                        if (prop.PropertyType == typeof(string))
                        {
                            sql = sql.Replace(name, $"'{prop.GetValue(param, null)}'");
                        }
                        else
                        {
                            sql = sql.Replace(name, prop.GetValue(param, null).ToString());
                        }
                    }
                }
                param = null;
            }

            return this.connection.Query<T>(sql, param, transaction, buffered, commandTimeout);
        }
        /// <summary>
        /// 返回结果集的第一个实体,不存在则返回null
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>结果集的第一个实体</returns>
        public virtual T QueryFirst<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            try
            {
                if (commandTimeout == null)
                    commandTimeout = this.CommanTimeout;

                // 使用参数化SQL语句GBase会报语法错误，现在此处将语句中参数替换为实际值
                if (DBType == ConnectType.GBase && param != null)
                {
                    var paramNames = sql.Split(' ').Where(s => s.Contains("=:")).Select(s => s.Substring("=")).ToList();
                    var paramType = param.GetType();
                    var propNames = paramType.GetProperties();
                    foreach (var name in paramNames)
                    {
                        var expectPropName = name.Replace(":", "").ToLower();
                        var prop = propNames.Where(n => n.Name.ToLower() == expectPropName).FirstOrDefault();
                        if (prop != null)
                        {
                            if (prop.PropertyType == typeof(string))
                            {
                                sql = sql.Replace(name, $"'{prop.GetValue(param, null)}'");
                            }
                            else
                            {
                                sql = sql.Replace(name, prop.GetValue(param, null).ToString());
                            }
                        }
                    }
                    param = null;
                }

                return this.connection.QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout);
            }
            catch (Exception ex)
            {
                return default(T);
            }
            //finally
            //{
            //    throw new Exception;
            //}
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">谓词表达式</param>
        /// <param name="sort">排序</param>
        /// <param name="page">第几页,页是从1开始的</param>
        /// <param name="resultsPerPage">每页数量</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="buffered">是否缓存</param>
        /// <returns>结果集</returns>
        public virtual IEnumerable<T> GetPageList<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, out int totalCount, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            if (page < 1) throw new ArgumentException($"{nameof(page)}必须大于或等于1");
            //获取总数
            totalCount = this.connection.Count<T>(predicate);
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            //Dapper-Extension的page是从0开始的，所以要减去1
            return this.connection.GetPage<T>(predicate, sort, page - 1, resultsPerPage, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="page">第几页，页是从1开始的</param>
        /// <param name="resultsPerPage">每页个数</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <param name="buffered">是否缓存</param>
        /// <returns>结果集</returns>
        public virtual IEnumerable<T> GetPageList<T>(string sql, int page, int resultsPerPage, out int totalCount, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            if (page < 1) throw new ArgumentException($"{nameof(page)}必须大于或等于1");

            // 使用参数化SQL语句GBase会报语法错误，现在此处将语句中参数替换为实际值
            if (DBType == ConnectType.GBase && param != null)
            {
                var paramNames = sql.Split(' ').Where(s => s.Contains("=:")).Select(s => s.Substring("=")).ToList();
                var paramType = param.GetType();
                var propNames = paramType.GetProperties();
                foreach (var name in paramNames)
                {
                    var expectPropName = name.Replace(":", "").ToLower();
                    var prop = propNames.Where(n => n.Name.ToLower() == expectPropName).FirstOrDefault();
                    if (prop != null)
                    {
                        if (prop.PropertyType == typeof(string))
                        {
                            sql = sql.Replace(name, $"'{prop.GetValue(param, null)}'");
                        }
                        else
                        {
                            sql = sql.Replace(name, prop.GetValue(param, null).ToString());
                        }
                    }
                }
                param = null;
            }

            //获取分页sql
            string pageSql = this.GetPageSql(sql, page, resultsPerPage);
            //获取总数
            totalCount = GetTotalCount(sql, param);
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Query<T>(pageSql, param, transaction, buffered, commandTimeout, CommandType.Text);
        }


        #endregion

        #region LambdaExtension
        /// <summary>
        /// 查询集合(表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">表达式</param>
        /// <param name="sort">排序字段</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="buffered">是否缓存</param>
        /// <returns>结果集</returns>
        public virtual IEnumerable<T> GetList<T>(Expression<Func<T, bool>> where, IList<ISort> sort = null,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            string sql;
            Dictionary<string, object> parameters;
            SelectExpression2Sql<T>(where, sort, out sql, out parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    dynamicParameters.Add(key, parameters[key]);
                }
            }
            Console.WriteLine("GetList + Expression  " + where);
            Console.WriteLine("GetList + Expression  " + sql);
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return connection.Query<T>(sql, parameters, transaction, buffered, commandTimeout, commandType);

        }

        /// <summary>
        /// 分页查询(表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">表达式</param>
        /// <param name="sort">排序字段</param>
        /// <param name="page">第几页</param>
        /// <param name="resultsPerPage">每页个数</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <param name="buffered">是否缓存</param>
        /// <returns>结果集</returns>
        public virtual IEnumerable<T> GetPageList<T>(Expression<Func<T, bool>> where, IList<ISort> sort,
            int page, int resultsPerPage, out int totalCount, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            string sql;
            Dictionary<string, object> parameters;
            SelectExpression2Sql<T>(where, sort, out sql, out parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    dynamicParameters.Add(key, parameters[key]);
                }
            }
            Console.WriteLine("GetPageList + Expression " + where);
            Console.WriteLine("GetPageList + Expression " + sql);
            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return GetPageList<T>(sql, page, resultsPerPage, out totalCount, dynamicParameters, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// 删除(表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        public virtual bool Delete<T>(Expression<Func<T, bool>> where, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql;
            Dictionary<string, object> parameters;
            DeleteExpression2Sql<T>(where, out sql, out parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    dynamicParameters.Add(key, parameters[key]);
                }
            }
            Console.WriteLine("Delete + Expression " + where);
            Console.WriteLine("Delete + Expression " + sql);

            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Execute(sql, dynamicParameters, transaction, commandTimeout) > 0;
        }

        /// <summary>
        /// 更新(表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="set">set语句的值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        public virtual bool Update<T>(Expression<Func<object>> set, Expression<Func<T, bool>> where, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql;
            Dictionary<string, object> parameters;
            UpdateExpression2Sql<T>(set, where, out sql, out parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    dynamicParameters.Add(key, parameters[key]);
                }
            }
            Console.WriteLine("Update + Expression " + where);
            Console.WriteLine("Update + Expression " + sql);

            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.Execute(sql, dynamicParameters, transaction, commandTimeout) > 0;
        }

        /// <summary>
        /// 查询数量(表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="select">查询字段值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>数量</returns>
        public virtual int Count<T>(Expression<Func<T, object>> select, Expression<Func<T, bool>> where, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql;
            Dictionary<string, object> parameters;
            CountExpression2Sql<T>(select, where, out sql, out parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    dynamicParameters.Add(key, parameters[key]);
                }
            }
            Console.WriteLine("Count + Expression " + where);
            Console.WriteLine("Count + Expression " + sql);

            if (commandTimeout == null)
                commandTimeout = this.CommanTimeout;
            return this.connection.ExecuteScalar<int>(sql, dynamicParameters, transaction, commandTimeout);
        }
        /// <summary>
        /// 创建可查询的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDAQueryable<T> CreateDAQuery<T>() where T : class
        {
            return new DAQueryable<T>(this);
        }
        #endregion

        #region ADO.Net Method

        /// <summary>
        /// 无查询执行
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="param">参数</param>
        /// <param name="isStoredProcedure">是否是存储过程</param>
        /// <param name="transaction">事务</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQuery(string sql, IDbDataParameter[] param = null, bool isStoredProcedure = false, IDbTransaction transaction = null)
        {
            try
            {
                //打开连接
                if (this.connection.State == ConnectionState.Closed)
                {
                    this.connection.Open();
                }

                IDbCommand cmd = this.GetCommand(sql, param);
                //是否是事务
                if (transaction != null)
                {
                    cmd.Transaction = transaction;
                }
                //是否是存储过程
                if (isStoredProcedure)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    cmd.CommandType = CommandType.Text;
                }

                //20200624 李双全 这里异步记录执行的command
                SqlLog.SqlLogger.OnLog(new SqlLog.SqlLogInfo(cmd));

                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {

            }
        }
        /// <summary>
        /// 执行查询，并返回结果集的第一行第一列，忽略额外的行列
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="param">参数</param>
        /// <param name="isStoredProcedure">是否是存储过程</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">执行超时</param>
        /// <returns>查询对象</returns>
        public virtual object ExecuteScalar(string sql, IDbDataParameter[] param = null, bool isStoredProcedure = false, IDbTransaction transaction = null, int commandTimeout = -1)
        {
            try
            {
                //打开连接
                if (this.connection.State == ConnectionState.Closed)
                {
                    this.connection.Open();
                }

                IDbCommand cmd = this.GetCommand(sql, param);
                //是否是事务
                if (transaction != null)
                {
                    cmd.Transaction = transaction;
                }
                //是否是存储过程
                if (isStoredProcedure)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    cmd.CommandType = CommandType.Text;
                }
                if (commandTimeout == -1)
                    commandTimeout = this.CommanTimeout;
                cmd.CommandTimeout = commandTimeout;

                //20200624 李双全 这里异步记录执行的command
                SqlLog.SqlLogger.OnLog(new SqlLog.SqlLogInfo(cmd));

                return cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
            }
        }
        /// <summary>
        /// 执行返回一个名为Table的DataTable
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="param">参数</param>
        /// <param name="isStoredProcedure">是否设成 <c>true</c> [是否是存储过程].</param>
        /// <param name="commandTimeout">执行超时</param>
        /// <returns>数据表</returns>
        [Obsolete("查询性能低下，不建议使用，请使用ExecuteReader")]
        public DataTable ExecuteTable(string sql, IDbDataParameter[] param = null, bool isStoredProcedure = false, int commandTimeout = -1)
        {
            DataSet ds = new DataSet();
            if (commandTimeout == -1)
                commandTimeout = this.CommanTimeout;
            Fill(sql, ds, param, isStoredProcedure, commandTimeout);
            return ds.Tables[0];
        }

        /// <summary>
        /// 执行返回DataReader
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="param">参数</param>
        /// <param name="behavior">连接说明</param>
        /// <param name="isStoredProcedure">是否设成 <c>true</c> [是否是存储过程].</param>
        /// <param name="commandTimeout">执行超时</param>
        /// <returns>数据表</returns>
        public IDataReader ExecuteReader(string sql, IDbDataParameter[] param = null, CommandBehavior behavior = CommandBehavior.CloseConnection, bool isStoredProcedure = false, int commandTimeout = -1, IDbTransaction transaction = null )
        {
            try
            {
                //打开连接
                if (this.connection.State == ConnectionState.Closed)
                {
                    this.connection.Open();
                }
                IDbCommand cmd = this.GetCommand(sql, param);
                //是否是事务
                if (transaction != null)
                {
                    cmd.Transaction = transaction;
                }
                if (isStoredProcedure)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    cmd.CommandType = CommandType.Text;
                }
                if (commandTimeout == -1)
                    commandTimeout = this.CommanTimeout;
                cmd.CommandTimeout = commandTimeout;

                //20200624 李双全 这里异步记录执行的command
                SqlLog.SqlLogger.OnLog(new SqlLog.SqlLogInfo(cmd));

                IDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region 公共方法
        /// <summary>
        /// 填充dataset,并生成一个名为Table的DataTable
        /// </summary>
        /// <param name="sql">查询SQL</param>
        /// <param name="dataset">dataset</param>
        /// <param name="param">参数</param>
        /// <param name="isStoredProcedure">是否是存储过程</param>
        /// <param name="commandTimeout">执行超时</param>
        /// <returns></returns>
        protected int Fill(string sql, DataSet dataset, IDataParameter[] param, bool isStoredProcedure, int commandTimeout)
        {
            try
            {
                //打开连接
                if (this.connection.State == ConnectionState.Closed)
                {
                    this.connection.Open();
                }

                IDbCommand cmd = this.GetCommand(sql, param);

                //是否是存储过程
                if (isStoredProcedure)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    cmd.CommandType = CommandType.Text;
                }

                cmd.CommandTimeout = commandTimeout;

                //20200624 李双全 这里异步记录执行的command
                SqlLog.SqlLogger.OnLog(new SqlLog.SqlLogInfo(cmd));

                IDbDataAdapter adapter = GetAdapter(cmd);

                return adapter.Fill(dataset);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {

            }
        }
        #endregion

        #region Dapper-Extension公共方法
        /// <summary>
        /// 获取表名（通过Dapper-Extension）
        /// </summary>
        /// <param name="t">泛型类型</param>
        /// <returns>表名</returns>
        public static string GetTableName(Type t)
        {
            IClassMapper classMap = PCI.Framework.ORM.DapperExtensions.DapperExtensions.Instance.SqlGenerator.Configuration.GetMap(t);

            return PCI.Framework.ORM.DapperExtensions.DapperExtensions.Instance.SqlGenerator.GetTableName(classMap);
        }
        /// <summary>
        /// 获取实体字段对应的数据库列名，包含车站.（通过Dapper-Extension）
        /// </summary>
        /// <param name="t">泛型类型</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns>车站.列名</returns>
        protected static string GetColumnName(Type t, string fieldName)
        {
            IClassMapper classMap = PCI.Framework.ORM.DapperExtensions.DapperExtensions.Instance.SqlGenerator.Configuration.GetMap(t);
            //获取到的列名前面会加 tableName.
            return PCI.Framework.ORM.DapperExtensions.DapperExtensions.Instance.SqlGenerator.GetColumnName(classMap, fieldName, false);
        }
        /// <summary>
        /// 获取实体字段对应的数据库列名，纯列名（通过Dapper-Extension）
        /// 当查询不到列名或者列名被忽略时，返回null
        /// </summary>
        /// <param name="t">泛型类型</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns>列名</returns>
        protected static string GetColumnNameOnly(Type t, string fieldName)
        {
            //string tbName = GetTableName(t);

            //return GetColumnName(t, fieldName).Substring(tbName.Length + 1);
            //lishuangquan Dapper-Extension返回的映射列名或者表名，都不要带前缀，统一由SQLBuilder处理
            IClassMapper classMap = PCI.Framework.ORM.DapperExtensions.DapperExtensions.Instance.SqlGenerator.Configuration.GetMap(t);
            var map = classMap.Properties.FirstOrDefault(x => x.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
            if (map == null || map.Ignored)//20200602 li 当属性被忽略时，返回null
            {
                return null;
            }
            return map.ColumnName;
        }

        #endregion

        #region lambda2sql
        /// <summary>
        /// 表达式转成sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="sort">排序</param>
        /// <param name="sql">返回参数化sql</param>
        /// <param name="parameters">参数</param>
        /// <returns>是否成功</returns>
        protected static bool SelectExpression2Sql<T>(Expression<Func<T, bool>> where, IList<ISort> sort, out string sql, out Dictionary<string, object> parameters) where T : class
        {
            SqlBuilderCore<T> builder = SqlBuilder.Select<T>();
            //20200602 li 修复当where为null时报错的问题
            if (where != null)
            {
                builder = builder.Where(where);
            }
            if (sort != null)
            {
                //生成表达式树
                Expression<Func<T, object>> orderBy;
                orderBy = t => sort.Select(a => GetColumnNameOnly(typeof(T), a.PropertyName));
                OrderType[] orderTypes = sort.Select(a => a.Ascending ? OrderType.Ascending : OrderType.Descending).ToArray();
                builder = builder.OrderBy(orderBy, orderTypes);
            }
            sql = builder.Sql;
            parameters = builder.Parameters;
            return true;
        }

        /// <summary>
        /// 表达式转成sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="sql">返回参数化sql</param>
        /// <param name="parameters">参数</param>
        /// <returns>是否成功</returns>
        protected static bool DeleteExpression2Sql<T>(Expression<Func<T, bool>> where, out string sql, out Dictionary<string, object> parameters) where T : class
        {
            SqlBuilderCore<T> builder = SqlBuilder.Delete<T>().Where(where);
            sql = builder.Sql;
            parameters = builder.Parameters;
            return true;
        }

        /// <summary>
        /// 表达式转成sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="set">set语句的值</param>
        /// <param name="sql">返回参数化sql</param>
        /// <param name="parameters">参数</param>
        /// <returns>是否成功</returns>
        protected static bool UpdateExpression2Sql<T>(Expression<Func<object>> set, Expression<Func<T, bool>> where, out string sql, out Dictionary<string, object> parameters) where T : class
        {
            SqlBuilderCore<T> builder = SqlBuilder.Update<T>(set).Where(where);
            sql = builder.Sql;
            parameters = builder.Parameters;
            return true;
        }

        /// <summary>
        /// 表达式转成sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="select">查询字段值</param>
        /// <param name="sql">返回参数化sql</param>
        /// <param name="parameters">参数</param>
        /// <returns>是否成功</returns>
        protected static bool CountExpression2Sql<T>(Expression<Func<T, object>> select, Expression<Func<T, bool>> where, out string sql, out Dictionary<string, object> parameters) where T : class
        {
            SqlBuilderCore<T> builder = SqlBuilder.Count<T>(select);
            if (where != null)
            {
                builder = builder.Where(where);
            }
            sql = builder.Sql;
            parameters = builder.Parameters;
            return true;
        }
        #endregion

        /// <summary>
        /// 实现释放
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.connection?.Close();
                this.connection?.Dispose();
            }
            catch
            {

            }
        }
    }
}

