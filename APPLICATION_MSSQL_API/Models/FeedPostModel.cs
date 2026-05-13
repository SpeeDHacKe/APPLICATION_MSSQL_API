namespace APPLICATION_MSSQL_API.Models
{
    public class FeedPostModel
    {
        public int id {  get; set; }
        public string? username {  get; set; }
        public DateTime postDate {  get; set; }
        public string? imageUrl {  get; set; }
        public List<CommentModel>? comments { get; set; }
    }
}
