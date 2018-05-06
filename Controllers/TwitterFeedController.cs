using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreDemo.Authorization;
using CoreDemo.Configuration;
using CoreDemo.DTOs;
using CoreDemo.Models;
using CoreDemo.Services.Twitter;
using LinqToTwitter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CoreDemo.Controllers
{
    [Route("TwitterFeed")]
    public class TwitterFeedController : Controller
    {
        private readonly ITwitterService _twitterService;
        private readonly TwitterConfig _twitterConfig;
        private readonly IAuthorizationManager<MvcAuthorizer> _authorizationManager;

        public TwitterFeedController(ITwitterService twitterService, 
            IAuthorizationManager<MvcAuthorizer> authorizationManager,
            IOptions<TwitterConfig> twitterConfig)
        {
            _authorizationManager = authorizationManager;
            _twitterService = twitterService;
            _twitterConfig = twitterConfig.Value;
        }

        [Route("Index")]
        [HttpGet]
        public ActionResult Index()
        {
            bool isAuthorized =_authorizationManager.IsAuthorized(HttpContext.Session);
            
            if (!isAuthorized)
            {
                return RedirectToAction("Index", "TwitterOAuth");
            }

            return View();
        }
        
        [Route("Feed/{pageIndex?}")]
        [HttpGet]
        public async Task<ActionResult> Feed(string hashTag, string searchTerm, bool filtering = false, int pageIndex = 0)
        {
            bool isAuthorized = _authorizationManager.IsAuthorized(HttpContext.Session);

            if (!isAuthorized)
            {
                return RedirectToAction("Index", "TwitterOAuth");
            }
            
            // Server side validation, in case the client side is removed by hand.
            if (string.IsNullOrEmpty(searchTerm) && string.IsNullOrEmpty(hashTag))
            {
                return View("Views/TwitterFeed/Error.cshtml", "The Search term can not be empty!");
            }

            PageTweetBorders pageTweetBorders = GetPageTweetBorders(pageIndex, filtering);
            int pageSize = _twitterConfig.PageSize;
            string query = $"{hashTag} {searchTerm}";

            // Unfortunatelly, twitter does not provide standart page parameters to do paging. 
            // Their idea is to provide since and max ids and leave it to the developer to implement their own paging.
            IList<TweetDto> tweetsResult = 
                await _twitterService.Search(query, pageSize, pageTweetBorders.SinceId, pageTweetBorders.MaxId);

            if (tweetsResult.Count > 0)
            {
                SetPageTweetBorders(pageIndex, tweetsResult);
            }
            
            TweetViewModel tweetsViewModel = 
                BuildTweetViewModel(tweetsResult, pageSize, pageIndex, hashTag, searchTerm, filtering);

            return View(tweetsViewModel);
        }

        #region Paging
        // The idea is to have a page dictionary in memory that holds SinceId and MaxId for every page
        // and use them as page borders so we can tell the twitter API 
        // form which to which Id to load the tweets for the specific page
        private void SetPageTweetBorders(int pageIndex, IList<TweetDto> tweetsResult)
        {
            var pages = new Dictionary<int, PageTweetBorders>();

            string pagesKey = $"tweet-pages";

            if (TempData.ContainsKey(pagesKey))
            {
                pages =
                     JsonConvert.DeserializeObject<Dictionary<int, PageTweetBorders>>(TempData[pagesKey].ToString());
            }

            if (!pages.ContainsKey(pageIndex))
            {
                pages.Add(pageIndex, new PageTweetBorders()
                {
                    Page = pageIndex,
                    MaxId = tweetsResult?.Max(tweet => tweet.Id) ?? 0,
                    SinceId = tweetsResult?.Min(tweet => tweet.Id) ?? 0
                });
            }

            string pagesJson = JsonConvert.SerializeObject(pages);

            if (TempData.ContainsKey(pagesKey))
            {
                TempData[pagesKey] = pagesJson;
            }
            else
            {
                TempData.Add(pagesKey, pagesJson);
            }
        }

        private PageTweetBorders GetPageTweetBorders(int pageIndex, bool filtering)
        {
            var result = new PageTweetBorders()
            {
                Page = 0,
                MaxId = 0,
                SinceId = 0
            };

            if (filtering) return result;

            string pagesKey = $"tweet-pages";
            if (TempData.ContainsKey(pagesKey))
            {
                var pages = 
                    JsonConvert.DeserializeObject<Dictionary<int, PageTweetBorders>>(TempData[pagesKey].ToString());
                
                // Prev Page
                if (pages.ContainsKey(pageIndex))
                {
                    result = pages[pageIndex];
                }
                // Next Page
                else if(pages.ContainsKey(pageIndex - 1))
                {
                    PageTweetBorders prevPage = pages[pageIndex - 1];

                    result = new PageTweetBorders()
                    {
                        Page = pageIndex,
                        MaxId = ulong.MaxValue,
                        SinceId = prevPage.MaxId
                    };
                }
            }

            return result;
        }
        #endregion

        private TweetViewModel BuildTweetViewModel(IList<TweetDto> tweetsResult, int pageSize, int pageIndex, string hashTag, string searchTerm, bool filtering)
        {
            IEnumerable<TweetModel> tweets = tweetsResult
                .Select(result => new TweetModel()
                {
                    Id = result.Id,
                    ScreenName = result.ScreenName,
                    Text = result.Text,
                    Selected = false,
                    CreatedAt = result.CreatedAt
                });

            TweetViewModel tweetsViewModel = new TweetViewModel()
            {
                Tweets = tweets,
                HasNextPage = (tweets.Count() >= 0 && tweets.Count() == pageSize),
                HasPrevPage = (pageIndex > 0),
                PageIndex = pageIndex,
                SearchModel = new SearchModel()
                {
                    HashTag = hashTag,
                    SearchTerm = searchTerm
                }
            };

            if(filtering)
            {
                tweetsViewModel.HasNextPage = true;
                tweetsViewModel.HasPrevPage = false;
            }
            
            return tweetsViewModel;
        }
    }

    public class PageTweetBorders
    {
        public int Page { get; set; }
        public ulong SinceId { get; set; }
        public ulong MaxId { get; set; }
    }
}