using APPLICATION_MSSQL_API.Common.Extensions;
using APPLICATION_MSSQL_API.DataAccess.Interfaces;
using APPLICATION_MSSQL_API.Models;
using Microsoft.Data.SqlClient;
using System.Collections;
using System.Data;

namespace APPLICATION_MSSQL_API.DataAccess
{
    public class DataCenter : IDataCenter
    {
        private string _conStr;
        private readonly string _readonly = "ReadOnly";
        private readonly string _readwrite = "ReadWrite";
        private readonly IHttpContextAccessor _context;
        public DataCenter(IConfiguration config, IHttpContextAccessor context)
        {
            _context = context;
            _conStr = config.GetConnectionString("DbContext") ?? string.Empty;
        }

        public SqlConnection GetOpenConnection()
        {
            SqlConnection connection = new SqlConnection(this._conStr);
            connection.Open();
            return connection;
        }

        public async Task<(bool success, string message, T data)> Get<T>(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) where T : new()
        {
            var result = new T();
            try
            {
                _ = CheckIsNullOrWhiteSpace(commandText);
                _conStr = _conStr.Replace(_readwrite, _readonly);
                var DS = _GetTest(commandText, type, param);
                result = DataSetToList<T>(DS);
                return await Task.FromResult((true, "success", result));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((false, ex.Message, result));
            }
        }

        public async Task<(bool success, string message, T1 data, T2 datas)> Gets<T1, T2>(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) where T1 : new() where T2 : new()
        {
            var result = new T1();
            var results = new T2();

            try
            {
                _ = CheckIsNullOrWhiteSpace(commandText);
                _conStr = _conStr.Replace(_readwrite, _readonly);
#if DEBUG
                var DS = _GetTest(commandText, type, param);
#else
                var DS = _Get(commandText, type, param);
#endif
                var dsResult = DataSetToLists<T1, T2>(DS);
                result = dsResult.Item1;
                results = dsResult.Item2;
                return await Task.FromResult((true, "success", result, results));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((false, ex.Message, result, results));
            }
        }

        public async Task<(bool success, string message, int numberRowsAffected)> ExecuteAsync(string commandText, CommandType commandType, IEnumerable<SqlParameter>? param = null)
        {
            (bool success, string message, int numberRowsAffected) result;
            try
            {
                _conStr = _conStr.Replace(_readonly, _readwrite);
                int numberRowsAffected = 0;                using (var connection = GetOpenConnection())
                {
                    SqlCommand cmd = new SqlCommand(commandText, connection);
                    cmd.CommandType = commandType;
                    if (param != null && param.Any())
                    {
                        cmd.Parameters.AddRange(param.ToArray());
                    }

                    numberRowsAffected = await cmd.ExecuteNonQueryAsync();
                }
                if (numberRowsAffected > 0) result = (true, $"{numberRowsAffected} rows affected.", numberRowsAffected);
                else result = (true, $"The command was executed but {numberRowsAffected} row affected.", numberRowsAffected);
            }
            catch (Exception ex)
            {
                result = (false, ex.Message, 0);
            }
            return await Task.FromResult(result);
        }

        public async Task<(bool success, string message)> Execute(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null)
        {
            (bool success, string message) result;
            try
            {
                _ = CheckIsNullOrWhiteSpace(commandText);
                _conStr = _conStr.Replace(_readonly, _readwrite);
                var afrows = await _ExecuteTest(commandText, type, param);
                if (afrows > 0) result = (true, "success");
                else result = (true, "The command was executed but 0 row affected.");
            }
            catch (Exception ex)
            {
                result = (false, ex.Message);
            }
            return await Task.FromResult(result);
        }

        public DataSet _GetTest(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) // for test on localhost
        {
            DataSet DS = new DataSet();
            using (SqlConnection _conn = new SqlConnection(_conStr))
            {
                _conn.Open();
                SqlDataAdapter sqlData = new SqlDataAdapter(commandText, _conn);
                sqlData.SelectCommand.CommandType = type;
                if (param != null && param.Any())
                {
                    foreach (var p in param)
                    {
                        sqlData.SelectCommand.Parameters.AddWithValue(p.ParameterName, p.Value);
                    }
                }
                sqlData.Fill(DS);
            }
            if (DS.Tables.Count < 1 || DS.Tables[0].Rows.Count < 1) throw new InvalidOperationException($"Data not found.");

            return DS;
        }

        public async Task<int> _ExecuteTest(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) // for test on localhost
        {
            int afrows = 0;
            using (SqlConnection _conn = new SqlConnection(_conStr))
            {
                _conn.Open();
                try
                {
                    SqlCommand sqlCmd = new SqlCommand(commandText, _conn);
                    sqlCmd.CommandType = type;
                    if (param != null && param.Any()) param.ToList().ForEach(p => sqlCmd.Parameters.AddWithValue(p.ParameterName, p.Value));
                    afrows = sqlCmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    /**/
                }
                _conn.Close();
            }
            return await Task.FromResult(afrows);
        }

        private T? DataSetToList<T>(DataSet DS) where T : new()
        {
            var result = new T();
            if (result is IList && result.GetType().IsGenericType)
            {
                result = Extension.ConvertToModel<T>(DS.Tables[0]);
            }
            else
            {
                result = Extension.ConvertToModel<List<T>>(DS.Tables[0]).FirstOrDefault();
            }
            return result;
        }

        private (T1?, T2?) DataSetToLists<T1, T2>(DataSet DS) where T1 : new() where T2 : new()
        {
            var result = new T1();
            var results = new T2();
            DataSet dsNew;

            if (DS.Tables[0].Rows.Count > 0)
            {
                dsNew = new DataSet();
                dsNew.Tables.Add(DS.Tables[0].Copy());
                result = DataSetToList<T1>(dsNew);
            }

            if (DS.Tables[1].Rows.Count > 0)
            {
                dsNew = new DataSet();
                dsNew.Tables.Add(DS.Tables[1].Copy());
                results = DataSetToList<T2>(dsNew);
            }
            return (result, results);
        }

        public async Task<(bool success, string message, T data)> GetItems<T>(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) where T : new()
        {
            var result = new T();
            try
            {
                var DS = await GetItemsByLocal(commandText, type, param);
                result = DataSetToList<T>(DS);
                return await Task.FromResult((true, "success", result));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((false, ex.Message, result));
            }
        }

        private async Task<DataSet> GetItemsByLocal(string commandText, CommandType type, IEnumerable<SqlParameter>? param = null) // for test on localhost
        {
            DataSet DS = new DataSet();
            using (SqlConnection _conn = new SqlConnection(_conStr))
            {
                _conn.Open();
                SqlDataAdapter sqlData = new SqlDataAdapter(commandText, _conn);
                sqlData.SelectCommand.CommandType = type;
                if (param != null && param.Any())
                {
                    foreach (var p in param)
                    {
                        sqlData.SelectCommand.Parameters.AddWithValue(p.ParameterName, p.Value);
                    }
                }
                sqlData.Fill(DS);
            }
            return await Task.FromResult(DS);
        }

        private async Task<DataSet> QueryByLocal(string commandText, CommandType commandType, IEnumerable<SqlParameter>? param = null)
        {
            DataSet DS = new DataSet();
            using (var connection = GetOpenConnection())
            {
                SqlDataAdapter adapter = new SqlDataAdapter(commandText, connection);
                adapter.SelectCommand.CommandType = commandType;
                if (param != null && param.Any())
                {
                    adapter.SelectCommand.Parameters.AddRange(param.ToArray());
                }

                adapter.Fill(DS);
            }
            return await Task.FromResult(DS);
        }

        private bool CheckIsNullOrWhiteSpace(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) throw new ArgumentException($"'{nameof(commandText)}' cannot be null or whitespace.", nameof(commandText));
            return false;
        }
    }
}