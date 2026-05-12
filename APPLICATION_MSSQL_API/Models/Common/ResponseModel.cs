namespace APPLICATION_MSSQL_API.Models
{
    public class ResponseModel
    {
        public bool success { get; set; }
        public int status { get; set; }
        public object? data { get; set; }
        public string? message { get; set; }
        public object? error { get; set; }
    }
}