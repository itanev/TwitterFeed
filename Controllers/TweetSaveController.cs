using CoreDemo.Configuration;
using CoreDemo.DTOs;
using CoreDemo.Services.Twitter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CoreDemo.Controllers
{
    [Route("tweet")]
    public class TweetSaveController : Controller
    {
        private readonly ITwitterService _twitterService;
        private readonly TwitterConfig _twitterConfig;

        public TweetSaveController(IOptions<TwitterConfig> twitterConfig,
            ITwitterService twitterService)
        {
            _twitterConfig = twitterConfig.Value;
            _twitterService = twitterService;
        }

        [Route("save/file")]
        public async Task<IActionResult> SaveToFile([FromForm]List<ulong> selectedTweetIds)
        {
            IList<TweetDto> result = await _twitterService.GetTweets(selectedTweetIds);
            
            using (StreamWriter writer = System.IO.File.AppendText(_twitterConfig.TweetsFilePath))
            {
                foreach (var tweet in result)
                {
                    string tweetText = $"{tweet.ScreenName} | {tweet.Text} | {tweet.CreatedAt}";
                    writer.WriteLine(tweetText);
                }
            }

            return Ok();
        }
    }
}