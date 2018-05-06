using CoreDemo.DTOs;
using CoreDemo.Repos;
using LinqToTwitter;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreDemo.Caching;
using CoreDemo.Mappings;

namespace CoreDemo.Services.Twitter
{
    public class TwitterService : ITwitterService
    {
        private readonly ITwitterRepo _twitterRepo;
        private readonly IStaticCacheManager _staticCacheManager;

        public TwitterService(ITwitterRepo twitterRepo, IStaticCacheManager staticCacheManager)
        {
            _twitterRepo = twitterRepo;
            _staticCacheManager = staticCacheManager;
        }

        public async Task<IList<TweetDto>> GetTweets(IList<ulong> selectedTweetIds)
        {
            string joinedIds = string.Join(",", selectedTweetIds);
            string tweetsKey = $"tweet-{joinedIds}";

            var result = new List<TweetDto>();

            if (_staticCacheManager.IsSet(tweetsKey))
            {
                List<TweetDto> tweetsFromCache = _staticCacheManager.Get<List<TweetDto>>(tweetsKey);
                result = tweetsFromCache;
            }
            else
            {
                var currentTweets =
                    await (from tweet in _twitterRepo.Status
                           where tweet.Type == StatusType.Lookup &&
                                 tweet.TweetIDs == joinedIds
                           select tweet)
                    .ToListAsync();
                
                if (currentTweets != null)
                {
                    result = currentTweets.Select(tweet => tweet.ToTweetDto()).ToList();

                    _staticCacheManager.Set(tweetsKey, result, 60);
                }
            }

            return await Task.FromResult(result);
        }

        public async Task<IList<TweetDto>> Search(string query, int pageCount, ulong sinceId, ulong maxId)
        {
            IList<TweetDto> tweetDtos;

            string tweetsKey = $"tweets-collection-{query}-{pageCount}-{sinceId}-{maxId}";
            if (_staticCacheManager.IsSet(tweetsKey))
            {
                IList<TweetDto> tweetsFromCache = _staticCacheManager.Get<IList<TweetDto>>(tweetsKey);
                tweetDtos = tweetsFromCache;
            }
            else
            {
                var statusList = new List<Status>();

                var searchStatusResponse =
                    await (from tweet in _twitterRepo.Search
                           where tweet.Type == SearchType.Search &&
                                 tweet.MaxID == maxId &&
                                 tweet.SinceID == sinceId &&
                                 tweet.Query == query &&
                                 tweet.Count == pageCount
                           select tweet)
                    .FirstOrDefaultAsync();

                if (searchStatusResponse != null)
                {
                    statusList.AddRange(searchStatusResponse.Statuses);
                }

                tweetDtos = statusList.Select(tweet => tweet.ToTweetDto()).ToList();

                _staticCacheManager.Set(tweetsKey, tweetDtos, 60);
            }

            return tweetDtos;
        }
    }
}