using System.ComponentModel.DataAnnotations;

namespace 文件映射进程间通信
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
