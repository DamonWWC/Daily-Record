namespace WebApplication1.Service
{
    public class Scope
    {
        private int num = 0;
       
        public Scope(Singleton singleton,Transient transient)
        {
                
        }
        public void DoSomething()
        {
            num++;
        }
    }
}
