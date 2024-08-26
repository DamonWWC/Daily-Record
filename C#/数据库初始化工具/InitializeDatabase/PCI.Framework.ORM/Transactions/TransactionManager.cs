using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace PCI.Framework.ORM.Transactions
{
    /// <summary>
    /// 事务范围管理器
    /// 封装TransactionScope，提供类似显示事务的一系列方法。
    /// 支持分布式事务，需要配置MSDTC    
    /// </summary>
    //public class TransactionManager : IDisposable
    //{
    //    private static readonly object obj = new object();
    //    /// <summary>
    //    /// 表示是否已经被回收
    //    /// </summary>
    //    private bool disposed;
    //    private TransactionScope scope;

    //    #region 构造函数

    //    /// <summary>
    //    /// 默认构造函数
    //    /// 默认超时时间为60秒
    //    /// </summary>
    //    public TransactionManager()
    //    {
    //        scope = new TransactionScope();
    //    }

    //    /// <summary>
    //    /// 构造函数
    //    /// 设置事务创建方式
    //    /// </summary>
    //    /// <param name="scopeOption">事务范围选项 
    //    ///     0：如果存在环境事务，则将事务添加到已有环境事务中，否则则创建新的事务；
    //    ///     1：不管是否存在环境事务，每次都创建新的事务；
    //    ///     2：如果存在环境事务，则不创建事务（非事务方式运行代码）</param>
    //    public TransactionManager(TransactionScopeOption scopeOption)
    //    {
    //        scope = new TransactionScope(scopeOption);
    //    }

    //    /// <summary>
    //    /// 构造函数
    //    /// 设置事务创建方式和超时
    //    /// TransactionScopeOption,使用默认值Required（该范围需要一个事务。如果已经存在环境事务，则使用该环境事务。否则，在进入范围之前创建新的事务。）
    //    /// </summary>
    //    /// <param name="scopeTimeout">设置超时时间 格式new TimeSpan(0, 0, 10)</param>
    //    /// <param name="scopeOption">事务范围选项  默认参数</param>
    //    public TransactionManager(TimeSpan scopeTimeout,
    //        TransactionScopeOption scopeOption = TransactionScopeOption.Required)
    //    {
    //        scope = new TransactionScope(scopeOption, scopeTimeout);
    //    }

    //    /// <summary>
    //    /// 构造函数
    //    /// 设置事务创建方式和事务的超时，隔离级别等信息
    //    /// </summary>
    //    /// <param name="transOptions">TransactionOptions对象里设置事务的隔离级别和超时信息</param>
    //    /// <param name="scopeOption">事务范围选项</param>
    //    public TransactionManager(TransactionOptions transOptions,
    //        TransactionScopeOption scopeOption = TransactionScopeOption.Required)
    //    {
    //        scope = new TransactionScope(scopeOption, transOptions);
    //    }

    //    /// <summary>
    //    /// 构造函数
    //    /// 设置依赖事务和超时，多线程共用同一事务的时候会用到依赖事务
    //    /// </summary>
    //    /// <param name="dependentTransaction">依赖事务，从主线程/父类线程克隆得到</param>
    //    /// <param name="scopeTimeout">超时时间</param>
    //    public TransactionManager(Transaction dependentTransaction, TimeSpan scopeTimeout)
    //    {
    //        scope = new TransactionScope(dependentTransaction, scopeTimeout);
    //    }

    //    #endregion

    //    #region 事务管理

    //    /// <summary>
    //    /// 开始事务
    //    /// </summary>
    //    public void Begin()
    //    {
    //        if (disposed)
    //            throw new ObjectDisposedException("Already disposed，can't start a new transaction");

    //        if (scope == null)
    //            scope = new TransactionScope();
    //    }

    //    /// <summary>
    //    /// 提交事务
    //    /// </summary>
    //    public void Commit()
    //    {
    //        if (disposed)
    //            throw new ObjectDisposedException("Already disposed，can't start a new transaction");

    //        if (scope == null)
    //            throw new InvalidOperationException();

    //        scope.Complete();

    //        Dispose();
    //    }

    //    /// <summary>
    //    /// 回滚事务
    //    /// </summary>
    //    public void RollBack()
    //    {
    //        Dispose();
    //    }

    //    #endregion


    //    /// <summary>
    //    /// Dispose实现，参照标准的IDispose接口实现
    //    /// </summary>
    //    /// <param name="disposing"></param>
    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (this.disposed)//如果已经被回收，就中断执行
    //            return;

    //        if (disposing)
    //        {
    //            //TODO:回收托管资源，调用IDisposable的Dispose()方法就可以
    //            //双重判断后在释放资源
    //            if (scope != null)
    //            {
    //                lock (obj)
    //                {
    //                    if (scope != null)
    //                    {
    //                        scope.Dispose();
    //                        scope = null;
    //                    }
    //                }
    //            }

    //            disposed = true;
    //        }
    //        //TODO：回收非托管资源，把之设置为null，等待CLR调用析构函数的时候回收

    //        this.disposed = true;
    //    }
    //    /// <summary>
    //    /// 释放资源
    //    /// 实现IDispose接口，这样也可以使用Using方式，达到隐式事务
    //    /// </summary>
    //    public void Dispose()
    //    {
    //        Dispose(true);//释放托管资源
    //        GC.SuppressFinalize(this);//告诉GC，不用再调用析构函数
    //    }

    //    /// <summary>
    //    /// 析构函数
    //    /// </summary>
    //    ~TransactionManager()
    //    {
    //        Dispose(false); //
    //    }
    //}
}
