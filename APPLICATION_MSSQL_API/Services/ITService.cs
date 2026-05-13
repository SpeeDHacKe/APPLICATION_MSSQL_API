using APPLICATION_MSSQL_API.DataAccess.Interfaces;
using APPLICATION_MSSQL_API.Models;
using APPLICATION_MSSQL_API.Services.Interfaces;
using Org.BouncyCastle.Ocsp;
using System;
using System.Data;
using System.Linq;

namespace APPLICATION_MSSQL_API.Services
{
    public class ITService : IITService
    {
        protected readonly IDataCenter _dc;
        public ITService(IDataCenter dc) => _dc = dc;

        public async Task<ResponseModel> GetListIT()
        {
            var response = new ResponseModel();
            try
            {
                var strCommand = @$"SELECT * FROM tb_Approval";
                var result = await _dc.Get<List<ITModel>>(strCommand, CommandType.Text);
                if (result.success || result.message.Contains("Data not found"))
                {
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

        public async Task<ResponseModel> UpdateListIT(List<ITModel> reqData)
        {
            var response = new ResponseModel();
            try
            {
                var reasonCase = string.Join(" ", reqData.Select(x => $"WHEN {x.id} THEN '{x.reason}'"));
                var statusCase = string.Join(" ", reqData.Select(x => $"WHEN {x.id} THEN '{x.status}'"));
                var ids = string.Join(",", reqData.Select(x => x.id));

                var strQuery = $@"UPDATE tb_Approval
                                    SET
                                        reason = CASE id
                                            {reasonCase}
                                        END,
                                        status = CASE id
                                            {statusCase}
                                        END
                                    WHERE id IN ({ids})";

                //var cmdText = $@"UPDATE dbo.tb_Approval
                //SET reason = '${reqData[0].reason}',
                //                status = '${reqData[0].status}'
                //            WHERE id IN ({ string.Join(",", reqData.Select(x => x.id)) })";

                //var cmdText = $@"UPDATE dbo.tb_Approval
                //                    SET reason = '{reqData.reason}',
                //                        status = '{reqData.status}'
                //                  WHERE id = {reqData.id}";
                var result = await _dc.Execute(strQuery, CommandType.Text);
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