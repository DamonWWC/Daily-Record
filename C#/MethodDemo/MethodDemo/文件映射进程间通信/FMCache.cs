using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace 文件映射进程间通信
{
    public static class FMCache
    {
        private const int MaxUsers = 1000;
        private const string MapName = "UserCacheMap";
        private const int UserSize = 104; // Assume each User takes up 104 bytes (4 bytes for Id + 100 bytes for Name)

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, int flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, int dwNumberOfBytesToMap);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static IntPtr _hMapFile = IntPtr.Zero;
        private static IntPtr _pBaseAddress = IntPtr.Zero;

        public static void Initialize()
        {
            _hMapFile = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, MaxUsers * UserSize, MapName);

            if (_hMapFile == IntPtr.Zero)
            {
                throw new Exception("Unable to create file mapping.");
            }

            _pBaseAddress = MapViewOfFile(_hMapFile, 0xF001F, 0, 0, MaxUsers * UserSize);

            if (_pBaseAddress == IntPtr.Zero)
            {
                throw new Exception("Unable to map view of file.");
            }
        }

        public static void AddUser(User user)
        {
            int offset = user.Id * UserSize;
            byte[] buffer = new byte[UserSize];

            BitConverter.GetBytes(user.Id).CopyTo(buffer, 0);
            Encoding.UTF8.GetBytes(user.Name.PadRight(100)).CopyTo(buffer, 4);

            Marshal.Copy(buffer, 0, IntPtr.Add(_pBaseAddress, offset), UserSize);
        }

        public static void RemoveUser(int id)
        {
            int offset = id * UserSize;
            byte[] buffer = new byte[UserSize];

            Marshal.Copy(buffer, 0, IntPtr.Add(_pBaseAddress, offset), UserSize);
        }

        public static User GetUserById(int id)
        {
            int offset = id * UserSize;
            byte[] buffer = new byte[UserSize];

            Marshal.Copy(IntPtr.Add(_pBaseAddress, offset), buffer, 0, UserSize);

            int userId = BitConverter.ToInt32(buffer, 0);
            string userName = Encoding.UTF8.GetString(buffer, 4, 100).TrimEnd('\0');

            if (userId == 0)
            {
                return null;
            }

            return new User { Id = userId, Name = userName };
        }
        public static User[] GetAllUsers()
        {
            User[] users = new User[MaxUsers];
            for (int i = 0; i < MaxUsers; i++)
            {
                users[i] = GetUserById(i);
            }
            return users;
        }
        public static User GetUserWithMaxId()
        {
            User maxUser = null;
            for (int i = 0; i < MaxUsers; i++)
            {
                User user = GetUserById(i);
                if (user != null)
                {
                    if (maxUser == null || user.Id > maxUser.Id)
                    {
                        maxUser = user;
                    }
                }
            }
            return maxUser;
        }

        public static int GetUserCount()
        {
            int count = 0;
            for (int i = 0; i < MaxUsers; i++)
            {
                if (GetUserById(i) != null)
                {
                    count++;
                }
            }
            return count;
        }

        public static void UpdateUser(User user)
        {
            int offset = user.Id * UserSize;
            byte[] buffer = new byte[UserSize];

            BitConverter.GetBytes(user.Id).CopyTo(buffer, 0);
            Encoding.UTF8.GetBytes(user.Name.PadRight(100)).CopyTo(buffer, 4);

            Marshal.Copy(buffer, 0, IntPtr.Add(_pBaseAddress, offset), UserSize);
        }

        public static void Cleanup()
        {
            if (_pBaseAddress != IntPtr.Zero)
            {
                UnmapViewOfFile(_pBaseAddress);
                _pBaseAddress = IntPtr.Zero;
            }

            if (_hMapFile != IntPtr.Zero)
            {
                CloseHandle(_hMapFile);
                _hMapFile = IntPtr.Zero;
            }
        }
    }
}
