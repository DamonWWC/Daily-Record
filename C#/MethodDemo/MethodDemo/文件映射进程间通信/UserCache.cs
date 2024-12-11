using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 文件映射进程间通信
{
    public class UserCache
    {
        static UserCache()
        {
            FMCache.Initialize();
        }

        public static void QueryDataList()
        {
            for (int i = 0; i < 1000; i++)
            {
                var user = FMCache.GetUserById(i);
                if (user != null)
                {
                    Console.WriteLine($"ID: {user.Id}, Name: {user.Name}");
                }
            }
        }
        public static User GetUserById(int id)
        {
            return FMCache.GetUserById(id);
        }

        public static void AddNewUser(User user)
        {
            FMCache.AddUser(user);
        }

        public static void RemoveUser(int id)
        {
            FMCache.RemoveUser(id);
        }

        public static void Cleanup()
        {
            FMCache.Cleanup();
        }

        public static int GetUserCount()
        {
            return FMCache.GetUserCount();
        }

    }
}
