using APPLICATION_MSSQL_API.Models;

namespace APPLICATION_MSSQL_API.Services.Interfaces
{
    public interface ICommentService
    {
        Task<ResponseModel> GetFeedPost();
        Task<ResponseModel> AddComment(CommentModel reqData);
    }
}