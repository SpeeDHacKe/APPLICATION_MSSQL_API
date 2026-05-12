using APPLICATION_MSSQL_API.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var aspnetcoreENV = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (string.IsNullOrEmpty(aspnetcoreENV)) throw new ArgumentException("Not found ASPNETCORE_ENVIRONMENT Variable");
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:5001");
AppSetting.Configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddOpenApi();
builder.Services.AddAutoScope();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews(option => option.Conventions.Add(new JwtAuthorizationConvention("JwtPolicy", Convert.ToBoolean(builder.Configuration["Jwt:Authen"]), builder?.Configuration["Jwt:ActionIgnore"]?.Split(',')))).AddNewtonsoftJson();


//Authen 1
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader()
    //.AllowCredentials()
    .Build());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = builder.Configuration["Jwt:Issuer"],
         ValidAudience = builder.Configuration["Jwt:Issuer"],
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
     };
 });
builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("JwtPolicy", builder =>
    {
        builder.RequireAuthenticatedUser();
        builder.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"{AppSetting.AssemblyName} ({AppSetting.DateModified}) - {aspnetcoreENV}", Version = AppSetting.AssemblyVersion });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = $@"Standard Authorization header using the Bearer scheme. Example: ""bearer [token]"" 
                        <br>Test Key (actor: Note, role: ADMIN) <br />
                        <br>Please copy text below and paste in value input. <br /> 
                        <textarea readonly style='height:150px;min-height:unset;'>
                            Bearer {aspnetcoreENV?.GetBearerSchem()}
                        </textarea>"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", $"{AppSetting.AssemblyName} v1"));
}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();

//app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



public static class AppSetting
{
    public static IConfiguration? Configuration { get; set; }
    public static string? DateModified { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("dd/MM/yyyy - HH:mm:ss", new System.Globalization.CultureInfo("en-EN", false));
    public static string? AssemblyName { get; set; } = Assembly.GetExecutingAssembly().GetName().Name;
    public static string? AssemblyVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    public static string? SystemMessage(string? code) => Configuration?[$"SystemMessage:{code?.ToUpper()}"];
}

public static class SetupServiceLifeTime
{
    public static void AddAutoScope(this IServiceCollection services)
    {
        List<Type> allType = new List<Type>();
        List<string> nsRange = new List<string> { $"{AppSetting.AssemblyName}.Services", $"{AppSetting.AssemblyName}.Common", $"{AppSetting.AssemblyName}.DataAccess" };
        nsRange.ForEach(n =>
        {
            List<Type> srvTyp = Assembly.GetExecutingAssembly().GetTypes()
                                    .Where(t => t.Namespace != null && t.Namespace.StartsWith(n))
                                    .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic).ToList();

            allType.AddRange(srvTyp);
        });

        if (!allType.IsEmpty())
        {
            allType.ForEach(clss =>
            {
                var intrf = clss.GetInterfaces().FirstOrDefault();
                if (intrf != null)
                {
                    services.AddScoped(intrf, clss);
                }
                else
                {
                    services.AddScoped(clss);
                }
            });
        }
    }
}

public class JwtAuthorizationConvention : IApplicationModelConvention
{
    private readonly string[] _actionIgnore;
    private readonly string _policy;
    private readonly bool _auth;

    public JwtAuthorizationConvention(string policy, bool auth, string[] actionIgnore)
    {
        _policy = policy;
        _auth = auth;
        _actionIgnore = actionIgnore;
    }

    public void Apply(ApplicationModel application)
    {
        if (_auth)
        {
            application.Controllers.ToList().ForEach(controller =>
            {
                var isController = controller.Selectors.Any(x => x.AttributeRouteModel?.Template != null
                                                        && x.AttributeRouteModel.Template.ToLower().StartsWith("api"));
                if (isController)
                {
                    controller.Actions.ToList().ForEach(action =>
                    {
                        var isActionAuthen = _actionIgnore == null || _actionIgnore?.Contains(action.ActionName.ToLower()) == false;
                        if (isActionAuthen)
                        {
                            action.Filters.Add(new AuthorizeFilter(_policy));
                        }
                    });
                }
            });
        }
    }
}

public static class NswagExtensions
{
    public static string GetBearerSchem(this string env)
    {
        string token = "";
        try
        {
            if (env.ToLower() == "development")
            {
                token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA5LzA5L2lkZW50aXR5L2NsYWltcy9hY3RvciI6IkVhc3lCdXkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvaGFzaCI6Ijc3MzcwNWE4LWQwNzgtNDk4NS1hYzZlLWE1ZGY4ZjcwMmRmZCIsImV4cCI6MjAyNzEzNzMwMCwibmJmIjoxNzQzMTQwNTAwLCJpc3MiOiJpbHNjcmVlbnN5c3RlbSIsImF1ZCI6Imlsc2NyZWVuc3lzdGVtIn0.3Uqp1fYq6jwMtFc5OTjebgRvhJJ695EMuVeZGp6cLd8";
            }
            else if (env.ToLower() == "uat")
            {
                token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA5LzA5L2lkZW50aXR5L2NsYWltcy9hY3RvciI6IkVhc3lCdXkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvaGFzaCI6IjMyOTY2MzYwLTBlMTgtNDM2YS1hZTA0LWU1NzRlOTA5YzgwZSIsImV4cCI6MjAyNzEzNzM1NCwibmJmIjoxNzQzMTQwNTU0LCJpc3MiOiJpbHNjcmVlbnN5c3RlbSIsImF1ZCI6Imlsc2NyZWVuc3lzdGVtIn0.0V4IKwwaT-T7kapcY5BNkFsHZdbXXMETIKEWUxSIKS0";
            }
            else if (env.ToLower() == "production")
            {
                token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA5LzA5L2lkZW50aXR5L2NsYWltcy9hY3RvciI6IkVhc3lCdXkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvaGFzaCI6IjczNTJlMTZjLWYyMDItNDFkYi1iNTViLTQ4NWEzY2E0MDQyMyIsImV4cCI6MjAyNzEzNzM2NCwibmJmIjoxNzQzMTQwNTY0LCJpc3MiOiJpbHNjcmVlbnN5c3RlbSIsImF1ZCI6Imlsc2NyZWVuc3lzdGVtIn0.thtMl3rWBu5iY8md7mbNE0OPVKFaC8tO8WbEpyF-9e4";
            }

            return token;
        }
        catch
        {
            return token;
        }
    }
}