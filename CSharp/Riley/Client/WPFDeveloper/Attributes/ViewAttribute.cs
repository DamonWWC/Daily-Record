using System;

namespace WPFDeveloper.Attributes
{
    /// <summary>
    /// 标记视图类，用于自动注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewAttribute : Attribute
    {
        /// <summary>
        /// 视图的唯一键
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 视图的显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 视图的分组
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// 视图的排序顺序
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 服务生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

        public ViewAttribute(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }
    }
}
