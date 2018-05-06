using System.Collections.Generic;

namespace CoreDemo.Models
{
    public class TweetViewModel
    {
        public IEnumerable<TweetModel> Tweets { get; set; }
        public bool HasPrevPage { get; set; }
        public bool HasNextPage { get; set; }
        public int PageIndex { get; set; }
        public SearchModel SearchModel { get; set; }
    }
}
