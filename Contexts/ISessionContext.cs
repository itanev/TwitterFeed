using Microsoft.AspNetCore.Http;

namespace CoreDemo.Contexts
{
    public interface ISessionContext
    {
        ISession Session {get;}
    }
}
