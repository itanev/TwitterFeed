using CoreDemo.Authorization;
using CoreDemo.Contexts;
using LinqToTwitter;

namespace CoreDemo.Repos
{
    public class TwitterRepo : ITwitterRepo
    {
        private readonly TwitterContext _context;

        public TwitterRepo(ISessionContext sessionContext, IAuthorizationManager<MvcAuthorizer> twitterAuthorizationManager)
        {
            MvcAuthorizer auth = twitterAuthorizationManager.GetAuthorizer(sessionContext.Session);

            _context = new TwitterContext(auth);
        }

        public TwitterQueryable<Search> Search => _context.Search;

        public TwitterQueryable<Status> Status => _context.Status;
    }
}
