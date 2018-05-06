using CoreDemo.DTOs;
using CoreDemo.Models;
using LinqToTwitter;

namespace CoreDemo.Mappings
{
    public static class MappingsExtensions
    {
        public static TweetModel ToTweetModel(this TweetDto dto)
        {
            var model = new TweetModel()
            {
                Id = dto.Id,
                ScreenName = dto.ScreenName,
                Text = dto.Text,
                Selected = false,
                CreatedAt = dto.CreatedAt
            };

            return model;
        }

        public static TweetDto ToTweetDto(this Status status)
        {
            var dto = new TweetDto()
            {
                Id = status.StatusID,
                ScreenName = status.User.ScreenNameResponse,
                Text = status.Text,
                CreatedAt = status.CreatedAt
            };

            return dto;
        }
    }
}
