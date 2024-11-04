namespace 隐式类型转换
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var aa = ImplicitOper1();
            double bb = new ImplicitOper(345);
            Console.WriteLine("Hello, World!");
        }

        static ImplicitOper ImplicitOper1()
        {
            return 33;
        }
    }

    public class ImplicitOper
    {
        public double Num { get; set; }
        public ImplicitOper(double num)
        {
            Num = num;
        }

        public static implicit operator double(ImplicitOper oper)
        {
            return oper.Num;
        }

        public static implicit operator ImplicitOper(double aa)
        {
            return new ImplicitOper(aa);
        }
    }

}
