using Microsoft.AspNetCore.Http;

namespace CoreDemo.Contexts
{
    public class SessionContext : ISessionContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ISession Session
        {
            get
            {
                return _httpContextAccessor.HttpContext.Session;
            }
        }
    }
}
