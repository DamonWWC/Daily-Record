using System;

namespace WPFDeveloper.Attributes
{
    /// <summary>
    /// 用于标记需要自动注册到依赖注入容器的服务
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ServiceRegistrationAttribute : Attribute
    {
        /// <summary>
        /// 服务生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

        /// <summary>
        /// 服务接口类型（如果不指定，则使用类本身）
        /// </summary>
        public Type? ServiceType { get; set; }

        /// <summary>
        /// 是否替换已存在的服务注册
        /// </summary>
        public bool ReplaceExisting { get; set; } = false;

        /// <summary>
        /// 初始化服务注册特性
        /// </summary>
        /// <param name="lifetime">服务生命周期</param>
        public ServiceRegistrationAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            Lifetime = lifetime;
        }

        /// <summary>
        /// 初始化服务注册特性
        /// </summary>
        /// <param name="serviceType">服务接口类型</param>
        /// <param name="lifetime">服务生命周期</param>
        public ServiceRegistrationAttribute(Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ServiceType = serviceType;
            Lifetime = lifetime;
        }
    }

    /// <summary>
    /// 服务生命周期枚举
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// 瞬态：每次请求都创建新实例
        /// </summary>
        Transient = 0,

        /// <summary>
        /// 作用域：在同一作用域内复用同一实例
        /// </summary>
        Scoped = 1,

        /// <summary>
        /// 单例：全局唯一实例
        /// </summary>
        Singleton = 2
    }
}
