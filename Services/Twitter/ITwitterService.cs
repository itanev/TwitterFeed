using CoreDemo.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreDemo.Services.Twitter
{
    public interface ITwitterService
    {
        Task<IList<TweetDto>> Search(string query, int pageCount, ulong sinceId, ulong maxId);
        Task<IList<TweetDto>> GetTweets(IList<ulong> selectedTweetIds);
    }
}
