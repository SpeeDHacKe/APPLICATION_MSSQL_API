using APPLICATION_MSSQL_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace APPLICATION_MSSQL_API.DataAccess.Interfaces
{
    public interface IDataCenter
    {
        public Task<(bool success, string message, T data)> Get<T>(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) where T : new();
        public Task<(bool success, string message, T1 data, T2 datas)> Gets<T1, T2>(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) where T1 : new() where T2 : new();
        public Task<(bool success, string message, T data)> GetItems<T>(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) where T : new();
        public Task<(bool success, string message)> Execute(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null);
        public Task<(bool success, string message, int numberRowsAffected)> ExecuteAsync(string commandText, CommandType commandType, IEnumerable<SqlParameter>? param = null);
        public SqlConnection GetOpenConnection();
    }
}