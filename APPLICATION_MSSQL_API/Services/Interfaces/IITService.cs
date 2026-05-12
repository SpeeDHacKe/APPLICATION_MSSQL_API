using APPLICATION_MSSQL_API.Models;

namespace APPLICATION_MSSQL_API.Services.Interfaces
{
    public interface IITService
    {
        Task<ResponseModel> GetListIT();
        Task<ResponseModel> UpdateListIT(List<ITModel> reqData);
    }
}