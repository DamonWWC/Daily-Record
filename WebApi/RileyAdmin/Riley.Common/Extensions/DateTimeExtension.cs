using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Riley.Common.Extensions
{
    public static class DateTimeExtension
    {
        public static readonly DateTime TimestampStart= new(1970, 1, 1, 0, 0, 0, 0);
        /// <summary>
        /// 转换为时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime dateTime, bool milliseconds = false)
        {
            var timestamp = dateTime.ToUniversalTime() - TimestampStart;
            return (long)(milliseconds ? timestamp.TotalMilliseconds : timestamp.TotalSeconds);
        }

        /// <summary>
        /// 获取周几
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string GetWeekName(this DateTime datetime)
        {
            var day = (int)datetime.DayOfWeek;
            var week=new string[] { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
            return week[day];
        }
    }
}
