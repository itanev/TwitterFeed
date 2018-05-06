using System;
using System.Threading.Tasks;
using CoreDemo.Configuration;
using LinqToTwitter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoreDemo.Authorization.Twitter
{
    public class AuthorizationManager<T> : IAuthorizationManager<MvcAuthorizer> where T : new()
    {
        protected MvcAuthorizer Auth { get; set; }

        private readonly TwitterConfig _twitterConfig;

        public AuthorizationManager(IOptions<TwitterConfig> twitterConfig)
        {
            _twitterConfig = twitterConfig.Value;
        }

        public MvcAuthorizer GetAuthorizer(ISession session)
        {
            if(Auth == null)
            {
                Auth = new MvcAuthorizer
                {
                    CredentialStore = new SessionStateCredentialStore(session)
                };
            }

            return Auth;
        }

        public bool IsAuthorized(ISession session)
        {
            MvcAuthorizer mvcAuthorizer = GetAuthorizer(session);

            bool isAuthorized = true;

            if(string.IsNullOrEmpty(mvcAuthorizer.CredentialStore.ConsumerKey) ||
               string.IsNullOrEmpty(mvcAuthorizer.CredentialStore.ConsumerSecret) || 
               string.IsNullOrEmpty(mvcAuthorizer.CredentialStore.OAuthToken) ||
               string.IsNullOrEmpty(mvcAuthorizer.CredentialStore.OAuthTokenSecret))
            {
                isAuthorized = false;
            }

            return isAuthorized;
        }

        public async Task<IActionResult> BeginAuthorizationAsync(ISession session, string callbackUrl)
        {
            SetAuthorizationOptions(session);
            
            return await Auth.BeginAuthorizationAsync(new Uri(callbackUrl));
        }

        public async Task CompleteAuthorizeAsync(ISession session, string responseUrl)
        {   
            MvcAuthorizer auth = GetAuthorizer(session);

            await auth.CompleteAuthorizeAsync(new Uri(responseUrl));

            // This is how you access credentials after authorization.
            // The oauthToken and oauthTokenSecret do not expire.
            // You can use the userID to associate the credentials with the user.
            // You can save credentials any way you want - database, 
            //   isolated storage, etc. - it's up to you.
            // You can retrieve and load all 4 credentials on subsequent 
            //   queries to avoid the need to re-authorize.
            // When you've loaded all 4 credentials, LINQ to Twitter will let 
            //   you make queries without re-authorizing.
            //
            //var credentials = auth.CredentialStore;
            //string oauthToken = credentials.OAuthToken;
            //string oauthTokenSecret = credentials.OAuthTokenSecret;
            //string screenName = credentials.ScreenName;
            //ulong userID = credentials.UserID;
        }

        private void SetAuthorizationOptions(ISession session)
        {
            Auth = new MvcAuthorizer
            {
                CredentialStore = new SessionStateCredentialStore(session)
                {
                    ConsumerKey = _twitterConfig.ConsumerKey,
                    ConsumerSecret = _twitterConfig.ConsumerSecret
                }
            };
        }
    }
}
