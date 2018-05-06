using System.Threading.Tasks;
using CoreDemo.Authorization;
using LinqToTwitter;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CoreDemo.Controllers
{
    public class TwitterOAuthController : Controller
    {
        private readonly IAuthorizationManager<MvcAuthorizer> _twitterAuthorizationManager;

        public TwitterOAuthController(
            IAuthorizationManager<MvcAuthorizer> twitterAuthorizationManager)
        {
            _twitterAuthorizationManager = twitterAuthorizationManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Begin()
        {
            string callbackUrl = Request.GetDisplayUrl().Replace("Begin", "Complete");
            IActionResult result = 
                await _twitterAuthorizationManager.BeginAuthorizationAsync(HttpContext.Session, callbackUrl);

            return result;
        }

        public async Task<ActionResult> Complete()
        {
            await _twitterAuthorizationManager.CompleteAuthorizeAsync(HttpContext.Session, Request.GetDisplayUrl());

            return RedirectToAction("Index", "TwitterFeed");
        }
    }
}