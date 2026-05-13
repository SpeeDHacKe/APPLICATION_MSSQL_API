using APPLICATION_MSSQL_API.DataAccess.Interfaces;
using APPLICATION_MSSQL_API.Models;
using APPLICATION_MSSQL_API.Services.Interfaces;
using System.Data;

namespace APPLICATION_MSSQL_API.Services
{
    public class CommentService : ICommentService
    {
        protected readonly IDataCenter _dc;
        public CommentService(IDataCenter dc) => _dc = dc;

        public async Task<ResponseModel> GetFeedPost()
        {
            var response = new ResponseModel();
            try
            {
                var strCommand = @$"SELECT * FROM tb_FeedPost";
                var result = await _dc.Get<FeedPostModel>(strCommand, CommandType.Text);
                if (result.success || result.message.Contains("Data not found"))
                {
                    var comments = await _dc.Get<List<CommentModel>>($"SELECT * FROM tb_FeedComment WHERE postId = {result.data.id}", CommandType.Text);
                    result.data.comments = comments.success ? comments.data : null;
                    response.status = 200;
                    response.success = true;
                    response.data = result.data;
                }
                else
                    response.status = 400;
                response.message = result.message;
            }
            catch (Exception ex)
            {
                response.error = ex;
                response.message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> AddComment(CommentModel reqData)
        {
            var response = new ResponseModel();
            try
            {
                var cmdText = $@"INSERT INTO tb_FeedComment (postId, username, commentText) VALUES ('{reqData.postId}', '{reqData.username}', '{reqData.commentText}')";         
                var result = await _dc.Execute(cmdText, CommandType.Text);
                if (result.success)
                {
                    response.success = result.success;
                    response.message = "Update Data Success";
                }
                else
                {
                    response.message = !string.IsNullOrEmpty(response.message) ? response.message + ", " + result.message : result.message;
                }
            }
            catch (Exception ex)
            {
                response.message = !string.IsNullOrEmpty(response.message) ? response.message + ", " + ex.Message : ex.Message;
            }

            return await Task.FromResult(response);
        }
    }
}