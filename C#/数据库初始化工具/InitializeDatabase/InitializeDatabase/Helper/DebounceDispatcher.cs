using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InitializeDatabase.Helper
{
    /// <summary>
    /// 防抖调度器 确保函数在一定时间间隔内以固定的频率执行
    /// </summary>
    public class DebounceDispatcher
    {
        private Timer timer;

        /// <summary>
        /// 函数防抖
        /// </summary>
        /// <param name="action">需要执行的<see cref="Action"/></param>
        /// <param name="delay">时间间隔/毫秒</param>
        public void Debounce(Action action, int delay)
        {
            timer?.Dispose();
            timer = new Timer((_) =>
            {
                action?.Invoke();
            }, null, delay, Timeout.Infinite);
        }
    }

}
