using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace InitializeDatabase.Helper
{
    public static class EnumHelper
    {
        /// <summary>
        /// 将枚举类型的所有成员转成列表类型
        /// </summary>
        /// <typeparam name="T">泛型类型，枚举</typeparam>
        /// <returns>枚举成员列表</returns>
        public static List<T> ToList<T>() where T : Enum
        {
            var type = typeof(T);

            return (from int index
                    in Enum.GetValues(type)
                    select (T)Enum.Parse(type, index.ToString()))
                    .ToList();
        }

        /// <summary>
        /// 将枚举类型的所有成员转成数组
        /// </summary>
        /// <typeparam name="T">泛型类型，枚举</typeparam>
        /// <returns>枚举数组</returns>
        public static T[] ToArray<T>() where T : Enum
        {
            return ToList<T>().ToArray();
        }

        /// <summary>
        /// 将枚举类型的所有成员转成可观察集合
        /// </summary>
        /// <typeparam name="T">泛型类型，枚举</typeparam>
        /// <returns>枚举可观察集合</returns>
        public static ObservableCollection<T> ToCollection<T>() where T : Enum
        {
            var collection = new ObservableCollection<T>();

            foreach (var item in ToList<T>())
            {
                collection.Add(item);
            }

            return collection;
        }

        /// <summary>
        /// 将object类型转换为枚举
        /// </summary>
        /// <typeparam name="T">泛型类型，枚举</typeparam>
        /// <param name="obj">需要转换的类型</param>
        /// <returns>转换后的枚举类型</returns>
        public static T ToEnum<T>(this object obj)
        {
            return (T)Enum.Parse(typeof(T), obj?.ToString());
        }

        /// <summary>
        /// 获取枚举的中文解释
        /// </summary>
        /// <typeparam name="T">泛型类型，枚举</typeparam>
        /// <param name="item">当前的枚举值</param>
        /// <returns>枚举的中文解释</returns>
        public static string GetDescription<T>(this T item) where T : Enum
        {
            var enumType = typeof(T);
            var name = Enum.GetName(enumType, item);
            var attributes = enumType.GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes.FirstOrDefault() as DescriptionAttribute)?.Description;
        }
        /// <summary>
        /// 获取枚举的属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumobj"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Enum enumobj) where T : Attribute
        {
            Type type = enumobj.GetType();
            Attribute attr;
            try
            {
                string enumName = Enum.GetName(type, enumobj);
                FieldInfo field = type.GetField(enumName);
                attr = field.GetCustomAttribute<T>(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return (T)attr;
        }
    }
}
