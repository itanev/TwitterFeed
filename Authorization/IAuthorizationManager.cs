using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CoreDemo.Authorization
{
    public interface IAuthorizationManager<T> where T : new()
    {
        T GetAuthorizer(ISession session);

        bool IsAuthorized(ISession session);

        Task<IActionResult> BeginAuthorizationAsync(ISession session, string callbackUrl);

        Task CompleteAuthorizeAsync(ISession session, string responseUrl);
    }
}
