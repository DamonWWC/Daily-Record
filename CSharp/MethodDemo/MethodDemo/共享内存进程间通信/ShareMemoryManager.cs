using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 共享内存进程间通信
{
    public class ShareMemoryManager<T> : IDisposable
    {

        private bool disposed = false;



        public ShareMemoryManager(string name,int sharedMemoryBaseSize)
        {
                if(!typeof(T).IsSerializable)
            {

            }
        }

        public void Dispose()
        {
            
        }
    }
}
