using APPLICATION_MSSQL_API.Models;
using APPLICATION_MSSQL_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APPLICATION_MSSQL_API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _sv;

        public CommentController(ICommentService sv)
        {
            _sv = sv;
        }

        [HttpGet("GetFeedPost", Name = "GetFeedPost")]
        public async Task<IActionResult> GetFeedPost()
        {
            ResponseModel returnResponse = new ResponseModel();
            try
            {
                returnResponse = await _sv.GetFeedPost();
                return await Task.FromResult(Ok(returnResponse.data));
            }
            catch (Exception ex)
            {
                returnResponse.message = $"{ex.Message} - {ex.InnerException}";
                return await Task.FromResult(BadRequest(returnResponse));
            }
        }

        [HttpPost("AddComment", Name = "AddComment")]
        public async Task<IActionResult> AddComment(CommentModel reqData)
        {
            ResponseModel returnResponse = new ResponseModel();
            try
            {
                returnResponse = await _sv.AddComment(reqData);
                return await Task.FromResult(Ok(returnResponse.data));
            }
            catch (Exception ex)
            {
                returnResponse.message = $"{ex.Message} - {ex.InnerException}";
                return await Task.FromResult(BadRequest(returnResponse));
            }
        }
    }
}