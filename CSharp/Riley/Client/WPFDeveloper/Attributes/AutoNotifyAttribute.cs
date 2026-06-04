using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFDeveloper.Attributes
{
    /// <summary>
    /// 标记需要自动生成INotifyPropertyChanged属性的字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AutoNotifyAttribute : Attribute
    {
        /// <summary>
        /// 自定义属性名，如果为空则使用字段名生成属性名
        /// </summary>
        public string? PropertyName { get; }

        /// <summary>
        /// 当属性变化时需要同时通知的其他属性
        /// </summary>
        public string[]? AlsoNotifyFor { get; set; }

        public AutoNotifyAttribute() { }

        public AutoNotifyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
