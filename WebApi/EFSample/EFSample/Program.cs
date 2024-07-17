using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFSample
{
    internal class Program
    {


        static void Main(string[] args)
        {
            DemoContext _context = new DemoContext();

            //var aa = _context.Apis.First();
            //var bb = _context.demos.ToArray();
            var sss = _context.AAA.ToArray();
            Console.WriteLine("Hello, World!");
        }
    }



    [Table("demo")]
    public class Api
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string? CreatedTime { get; set; } = null;
    }
    [Table("ALARM_PLAN")]
    public class AA
    {
        [Key]
        public int PKEY { get; set; }
        public string ALARMID { get; set; }
        public int PLANID { get; set; }
        public int OPERATORID { get; set; }
        public DateTime CREATETIME { get; set; }
    }
    public class A
    {
        public int ROLE_ID { get; set; }

        public int RESOURCE_ID { get; set; }
        public int IS_ORG_RESOURCE { get; set; }

        public int CREATE_BY { get; set; }

       
    }
    public class DemoContext : DbContext
    {
        public DbSet<AA> demos { get; set; }

        public DbSet<A> AAA { get; set; }

        public DbSet<Api> Apis { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseDm("Server=172.25.11.183;Port=5236;Database=micsDB;User Id=MICS;PWD=NBL7mics;");

            optionsBuilder.UseDm("Server=172.25.11.183;Port=5236;Database=HJMOS_PERMISSIONS;User Id=MICS;PWD=NBL7mics;");


            // optionsBuilder.UseMySQL("Server=119.23.227.72; Port=3307; Database=admin; Uid=root; Pwd=123456; Charset=utf8mb4;SslMode=none;Min pool size=1");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<AA>().HasKey(e => e.PKEY);

            modelBuilder.Entity<A>().ToTable("SYS_ROLE_RESOURCE").HasKey(e => e.ROLE_ID);
        }
    }


}
