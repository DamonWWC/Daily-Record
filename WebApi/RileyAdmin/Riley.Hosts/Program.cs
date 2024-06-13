using Riley.Admin.Core;

namespace Riley.Hosts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new HostApp().Run(args, typeof(Program).Assembly);
        }

    }
}
