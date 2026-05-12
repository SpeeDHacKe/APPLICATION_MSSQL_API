using APPLICATION_MSSQL_API.DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APPLICATION_MSSQL_API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly IDataCenter _dc;

        public HealthCheckController(IDataCenter dc)
        {
            _dc = dc;
        }

        [HttpGet(Name = "APIHealthCheck")]
        public object APIHealthCheck()
        {
            try
            {
                return Ok(new { status = 200, success = true, message = $"Connect {AppSetting.AssemblyName} Success!" });
            }
            catch (Exception ex)
            {
                return NotFound(new { status = 500, success = false, message = $"{ex.Message} | Inner: {ex.InnerException?.Message}" });
            }
        }

        [HttpGet("ConnectionCheck", Name = "ConnectionCheck")]
        public object ConnectionCheck()
        {
            try
            {
                using (var con = _dc.GetOpenConnection())
                    return Ok(new { status = 200, success = true, message = $"Connect to DB Success!" });
            }
            catch (Exception ex)
            {
                return NotFound(new { status = 500, success = false, message = $"{ex.Message} | Inner: {ex.InnerException?.Message}" });
            }
        }
    }
}