using System;

namespace CoreDemo.Models
{
    public class TweetModel
    {
        public ulong Id { get; set; }

        public string ScreenName { get; set; }
        
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public bool Selected { get; set; }
    }
}
