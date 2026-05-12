using APPLICATION_MSSQL_API.Models;
using APPLICATION_MSSQL_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APPLICATION_MSSQL_API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ITController : ControllerBase
    {
        private readonly IITService _sv;

        public ITController(IITService sv)
        {
            _sv = sv;
        }

        [HttpGet("LoadData", Name = "LoadData")]
        public async Task<IActionResult> LoadData()
        {
            try
            {
                var returnResponse = await _sv.GetListIT();
                return await Task.FromResult(Ok(returnResponse.data));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(BadRequest(ex));
            }
        }

        [HttpGet("GetListIT", Name = "GetListIT")]
        public async Task<IActionResult> GetListIT()
        {
            ResponseModel returnResponse = new ResponseModel();
            try
            {
                returnResponse = await _sv.GetListIT();
                return await Task.FromResult(Ok(returnResponse));
            }
            catch (Exception ex)
            {
                returnResponse.message = $"{ex.Message} - {ex.InnerException}";
                return await Task.FromResult(BadRequest(returnResponse));
            }
        }

        [HttpPost("UpdateListIT", Name = "UpdateListIT")]
        public async Task<ResponseModel> UpdateListIT(List<ITModel> reqData)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                response = await _sv.UpdateListIT(reqData);
                return response;
            }
            catch (Exception ex)
            {
                response.message = $"{ex.Message} - {ex.InnerException}";
                return response;
            }
        }
    }
}