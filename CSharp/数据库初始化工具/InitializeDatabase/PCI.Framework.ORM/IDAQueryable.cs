using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PCI.Framework.ORM
{
    /// <summary>
    /// 可查询对象的接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDAQueryable<T> where T : class
    {
        /// <summary>
        /// AndWhere对主表的条件
        /// </summary>
        /// <param name="andWhere"></param>
        /// <returns></returns>
        IDAQueryable<T> AndWhere(Expression<Func<T, bool>> andWhere);

        /// <summary>
        /// AndWhere对其他表的条件
        /// </summary>
        /// <typeparam name="TModel">其他表的类型</typeparam>
        /// <param name="andWhere">条件</param>
        /// <returns></returns>
        IDAQueryable<T> AndWhere<TModel>(Expression<Func<TModel, bool>> andWhere);

        /// <summary>
        /// 两个表的关联
        /// </summary>
        /// <typeparam name="TModel1">与主表关联的其他表对应的实体类型</typeparam>
        /// <param name="where">关联关系</param>
        /// <returns></returns>
        IDAQueryable<T> LeftJoin<TModel1>(Expression<Func<T, TModel1, bool>> where)
            where TModel1 : class;

        /// <summary>
        /// 针对其他表的或条件
        /// </summary>
        /// <typeparam name="TModel">其他表对应的实体类型</typeparam>
        /// <param name="orWhere"></param>
        /// <returns></returns>
        IDAQueryable<T> OrWhere<TModel>(Expression<Func<TModel, bool>> orWhere) where TModel : class;

        /// <summary>
        /// 针对主表的或条件
        /// </summary>
        /// <param name="orWhere">条件</param>
        /// <returns></returns>
        IDAQueryable<T> OrWhere(Expression<Func<T, bool>> orWhere);

        /// <summary>
        /// 执行查询，返回dynamic集合
        /// </summary>
        /// <returns></returns>
        IEnumerable<dynamic> QueryAsDynamic();

        /// <summary>
        /// 执行查询，返回泛型集合
        /// </summary>
        /// <typeparam name="TResult">要返回的类型</typeparam>
        /// <returns>泛型集合</returns>
        IEnumerable<TResult> QueryAsList<TResult>() where TResult : class;

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        /// <returns></returns>
        DataTable QueryAsDataTable();

        /// <summary>
        /// 查询T,TModel1中的列
        /// </summary>
        /// <typeparam name="TModel1">其他表对应的实体类型</typeparam>
        /// <param name="select">要查询的列</param>
        /// <returns></returns>
        IDAQueryable<T> Select<TModel1>(Expression<Func<T, TModel1, object>> select) where TModel1 : class;

        /// <summary>
        /// 查询T中的列，select为空则选取全部列
        /// </summary>
        /// <param name="select">要查询的列，若为空，则选取全部列</param>
        /// <returns></returns>
        IDAQueryable<T> Select(Expression<Func<T, object>> select = null);

        /// <summary>
        /// 查询T,TModel1, TModel2中的列
        /// </summary>
        /// <typeparam name="TModel1">其他表1对应的实体类型</typeparam>
        /// <typeparam name="TModel2">其他表2对应的实体类型</typeparam>
        /// <param name="select"></param>
        /// <returns></returns>
        IDAQueryable<T> Select<TModel1, TModel2>(Expression<Func<T, TModel1, TModel2, object>> select)
            where TModel1 : class
            where TModel2 : class;

        /// <summary>
        /// 针对其他表对应的where条件
        /// </summary>
        /// <typeparam name="TModel">其他表对应的实体类型</typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        IDAQueryable<T> Where<TModel>(Expression<Func<TModel, bool>> where)
            where TModel : class;

        /// <summary>
        /// 针对主表的where条件
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IDAQueryable<T> Where(Expression<Func<T, bool>> where);

        /// <summary>
        /// 针对主表字段排序
        /// </summary>
        /// <param name="expression">选择的排序列</param>
        /// <returns></returns>
        IDAQueryable<T> OrderBy(Expression<Func<T, object>> expression);
        /// <summary>
        /// 针对主表单个字段排序(降序)
        /// </summary>
        /// <param name="expression">选择的排序列</param>
        /// <returns></returns>
        IDAQueryable<T> OrderByDesc(Expression<Func<T, object>> expression);

        /// <summary>
        /// 针对T2表字段排序
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        IDAQueryable<T> OrderBy<T2>(Expression<Func<T2, object>> expression);
        /// <summary>
        /// 针对T2表字段排序(降序)
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        IDAQueryable<T> OrderByDesc<T2>(Expression<Func<T2, object>> expression);

        /// <summary>
        /// 针对SQL语句去重
        /// </summary>
        /// <returns></returns>
        IDAQueryable<T> Distinct();
        /// <summary>
        /// 按页查询，页码从1开始，返回泛型集合
        /// </summary>
        /// <typeparam name="TResult">返回的泛型集合的类型</typeparam>
        /// <param name="page">页码，从1开始</param>
        /// <param name="pageSize">每页的最大数量</param>
        /// <param name="totalCount">查询的总数</param>
        /// <returns>当页的泛型集合</returns>
        IEnumerable<TResult> QueryPageAsList<TResult>(int page, int pageSize, out int totalCount) where TResult : class;

    }
}
