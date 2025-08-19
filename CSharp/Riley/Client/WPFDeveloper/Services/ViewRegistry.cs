using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WPFDeveloper.Views;

namespace WPFDeveloper.Services
{
    public interface IViewRegistry
    {
        Type? ResolveViewType(string viewTypeName);
        void RegisterView(string key, Type viewType);
        IEnumerable<string> GetRegisteredViewKeys();
    }

    public class ViewRegistry : IViewRegistry
    {
        private readonly Dictionary<string, Type> _viewTypeMap = new(StringComparer.OrdinalIgnoreCase);

        public ViewRegistry()
        {
            // 自动注册所有视图类型
            AutoRegisterViews();
        }

        private void AutoRegisterViews()
        {
            // 获取当前程序集中的所有视图类型
            var viewTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("View"));

            foreach (var viewType in viewTypes)
            {
                // 使用完整的类型名称作为键
                var key = viewType.FullName;
                if (!string.IsNullOrEmpty(key))
                {
                    RegisterView(key, viewType);
                }

                // 也使用类名作为键（去掉 View 后缀）
                var shortKey = viewType.Name.Replace("View", "");
                if (!string.IsNullOrEmpty(shortKey))
                {
                    RegisterView(shortKey, viewType);
                }
            }
        }

        public Type? ResolveViewType(string viewTypeName)
        {
            if (string.IsNullOrEmpty(viewTypeName))
                return null;

            // 首先尝试直接查找
            if (_viewTypeMap.TryGetValue(viewTypeName, out var viewType))
                return viewType;

            // 如果找不到，尝试通过反射查找类型
            try
            {
                var type = Type.GetType(viewTypeName);
                if (type != null)
                {
                    RegisterView(viewTypeName, type);
                    return type;
                }
            }
            catch
            {
                // 忽略类型解析错误
            }

            return null;
        }

        public void RegisterView(string key, Type viewType)
        {
            if (string.IsNullOrEmpty(key) || viewType == null)
                return;

            if (!_viewTypeMap.ContainsKey(key))
            {
                _viewTypeMap[key] = viewType;
            }
        }

        public IEnumerable<string> GetRegisteredViewKeys()
        {
            return _viewTypeMap.Keys.ToList();
        }
    }
}
