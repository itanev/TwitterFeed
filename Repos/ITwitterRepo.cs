using LinqToTwitter;

namespace CoreDemo.Repos
{
    public interface ITwitterRepo
    {
        TwitterQueryable<Search> Search { get; }
        TwitterQueryable<Status> Status { get; }
    }
}
