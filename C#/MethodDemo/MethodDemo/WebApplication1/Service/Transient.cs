namespace WebApplication1.Service
{
    public class Transient:ITransient
    {
        private readonly Scope _scope;

        public Transient(Scope scope)
        {
            _scope = scope;  
        }

        public void DoSomething()
        {
            _scope.DoSomething();
        }
    }
}
