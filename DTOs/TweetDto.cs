using System;

namespace CoreDemo.DTOs
{
    public class TweetDto
    {
        public ulong Id { get; set; }

        public string ScreenName { get; set; }
        
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
