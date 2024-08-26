using PCI.Framework.ORM.DapperExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace PCI.Framework.ORM
{
    /// <summary>
    /// 数据访问门面接口
    /// 
    /// 目的：
    ///     1.提供统一入口，用于对象映射，数据持久化和数据访问；
    ///     2.封装ADO.NET + Dapper + Dapper-Extensions；
    ///    
    /// 规范：
    ///     DataAccess Facade使用外观模式（Facade）统一外部对多种数据库的访问
    /// </summary> 
    public interface IDAFacade:IDisposable
    {
        /// <summary>
        /// 参数化sql的变量前缀
        /// </summary>
        string Prefix { get; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        ConnectType DBType { get; }
        /// <summary>
        /// 数据库连接
        /// </summary>
        DbConnection Connection { get; }
        /// <summary>
        /// 创建事务
        /// </summary>
        /// <returns>事务对象</returns>
        IDbTransaction CreateTransaction();
        /// <summary>
        /// 获取适配器
        /// </summary>
        /// <param name="cmd">数据库访问命令</param>
        /// <returns>适配器接口</returns>
        IDbDataAdapter GetAdapter(IDbCommand cmd);
        /// <summary>
        /// 创建命令
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">参数</param>
        /// <returns>sql命令</returns>
        IDbCommand GetCommand(string sql, IDataParameter[] para);
        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="type">类型</param>
        /// <param name="size">参数长度</param>
        /// <param name="direction">参数方向</param>
        /// <returns>参数接口</returns>
        IDbDataParameter GetParameter(string name, object value, DbType type = DbType.String, int size = 100, ParameterDirection direction = ParameterDirection.Input);

        #region Dapper-Extensions
        /// <summary>
        /// 查询数量
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">过滤条件谓词</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>数量</returns>
        int Count<T>(object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
        /// <summary>
        /// 新增
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>主键</returns>
        object Insert<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
        /// <summary>
        /// 新增多个
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="entities">实体集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>主键</returns>
        void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
        /// <summary>
        /// 根据主键获取
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="id">主键</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>实体</returns>
        T Get<T>(object id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
        /// <summary>
        /// 获取全部
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">过滤条件谓词</param>
        /// <param name="sort">排序</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>结果集</returns>
        IEnumerable<T> GetList<T>(object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
        /// <summary>
        /// 根据实体删除
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        bool Delete<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
        /// <summary>
        /// 根据谓词条件删除
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">谓词删除条件,使用IList IPredicate和IPredicateGroup</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        bool Delete<T>(object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
        /// <summary>
        /// 根据sql传参删除
        /// </summary>
        /// <param name="sql">delete语句</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        bool Delete(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null);
        /// <summary>
        /// 根据实体更新
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="ignoreAllKeyProperties">不需更新的字段</param>
        /// <returns>是否成功</returns>
        bool Update<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>结果集</returns>
        IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
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
        T QueryFirst<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">谓词表达式</param>
        /// <param name="sort">排序</param>
        /// <param name="page">第几页</param>
        /// <param name="resultsPerPage">每页数量</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="buffered">是否缓存</param>
        /// <returns>结果集</returns>
        IEnumerable<T> GetPageList<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, out int totalCount, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class;
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="page">第几页</param>
        /// <param name="resultsPerPage">每页个数</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <param name="buffered">是否缓存</param>
        /// <returns>结果集</returns>
        IEnumerable<T> GetPageList<T>(string sql, int page, int resultsPerPage, out int totalCount, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class;
        /// <summary>
        /// 创建可查询的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDAQueryable<T> CreateDAQuery<T>() where T:class;
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
        IEnumerable<T> GetList<T>(Expression<Func<T, bool>> where, IList<ISort> sort = null,
           IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) where T : class;
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
        IEnumerable<T> GetPageList<T>(Expression<Func<T, bool>> where, IList<ISort> sort,
            int page, int resultsPerPage, out int totalCount, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class;

        /// <summary>
        /// 删除(表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        bool Delete<T>(Expression<Func<T, bool>> where, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;

        /// <summary>
        /// 更新(表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="set">set语句的值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>是否成功</returns>
        bool Update<T>(Expression<Func<object>> set, Expression<Func<T, bool>> where, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;

        /// <summary>
        /// 查询数量(表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="where">where条件</param>
        /// <param name="select">查询字段值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns>数量</returns>
        int Count<T>(Expression<Func<T, object>> select, Expression<Func<T, bool>> where, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;

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
        int ExecuteNonQuery(string sql, IDbDataParameter[] param = null, bool isStoredProcedure = false, IDbTransaction transaction = null);

        /// <summary>
        /// 执行查询，并返回结果集的第一行第一列，忽略额外的行列
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="param">参数</param>
        /// <param name="isStoredProcedure">是否是存储过程</param>
        /// <param name="commandTimeout">执行超时</param>
        /// <param name="transaction">事务</param>
        /// <returns>查询对象</returns>
        object ExecuteScalar(string sql, IDbDataParameter[] param = null, bool isStoredProcedure = false, IDbTransaction transaction = null, int commandTimeout = -1);
        /// <summary>
        /// 执行返回一个名为Table的DataTable
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="param">参数</param>
        /// <param name="isStoredProcedure">是否设成 <c>true</c> [是否是存储过程].</param>
        /// <param name="commandTimeout">执行超时</param>
        /// <returns>数据表</returns>
        [Obsolete("查询性能低下，不建议使用，请使用ExecuteReader")]
        DataTable ExecuteTable(string sql, IDbDataParameter[] param = null, bool isStoredProcedure = false, int commandTimeout = -1);
        /// <summary>
        /// 执行返回DataReader
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="param">参数</param>
        /// <param name="behavior">连接说明</param>
        /// <param name="isStoredProcedure">是否设成 <c>true</c> [是否是存储过程].</param>
        /// <param name="commandTimeout">执行超时</param>
        /// <returns>数据表</returns>
        IDataReader ExecuteReader(string sql, IDbDataParameter[] param, CommandBehavior behavior = CommandBehavior.CloseConnection, bool isStoredProcedure = false, int commandTimeout = -1, IDbTransaction transaction = null);
        #endregion
    }
}
