using Microsoft.AspNetCore.Mvc;
using LinqToTwitter;

namespace CoreDemo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!new SessionStateCredentialStore(HttpContext.Session).HasAllCredentials())
                return RedirectToAction("Index", "TwitterOAuth");

            return View();
        }
    }
}
