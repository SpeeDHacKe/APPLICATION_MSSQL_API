using APPLICATION_MSSQL_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace APPLICATION_MSSQL_API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new IndexModel
            {
                IP = Dns.GetHostEntry(Dns.GetHostName())?.HostName ?? "",
                AssemblyName = AppSetting.AssemblyName,
                AssemblyVersion = AppSetting.AssemblyVersion,
                AspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                DateModified = AppSetting.DateModified
            });
        }
    }
}