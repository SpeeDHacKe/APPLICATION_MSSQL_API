namespace APPLICATION_MSSQL_API.Models
{
    public class CommentModel
    {
        public int id { get; set; }
        public int postId { get; set; }
        public string? username { get; set; }
        public string? commentText { get; set; }
        public DateTime createdDate { get; set; }
    }
}
