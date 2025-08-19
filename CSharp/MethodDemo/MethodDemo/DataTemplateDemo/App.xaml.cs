using DataTemplateDemo.Views;
using Prism.Ioc;
using System.Windows;

namespace DataTemplateDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            //Foo foo = null;
            //Foo foo2 = new Foo();
            //int n =   foo?.N+ ABC() + foo2?.N ?? 1;

            object a = null;
            B c = null;
            var aa = a is B b & c;

            a = new B();
            var bb = a is B b5 & c;


        }
        private int ABC()
        {
            return 2;
        }
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }


    class B
    {
        public static int operator &(B left, B right) => 1;
        public static int operator >(B left, B right) => 2;
        public static int operator <(B left, B right) => 3;

        public static int operator &(bool left, B right) => 5;
        public static int operator >(bool left, B right) => 6;
        public static int operator <(bool left, B right) => 7;
    }
    public class Foo
    {
        public int N { get; } = 1;
    }
}
