using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using System;
using System.IO;


public static class Program
{
    public static void Main(string[] args)
    {
        Server.Start(args);
    }
}

class Server
{
    public static void Start(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var rootFolder = Path.Combine(new string[] { AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "webroot" });

                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://localhost:8080/");
                webBuilder.UseWebRoot(rootFolder);
            });
}


public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    // This code configures Web API. The Startup class is specified as a type
    // parameter in the WebApp.Start method.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseDefaultFiles();
        app.UseStaticFiles();
    }
}

// https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    class TestResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public int id { get; set; }
    }

    [HttpGet]
    [Route("test/{id}")]
    public IActionResult Test(int id)
    {
        return Ok(new TestResponse() { code = "helloworld", message = "Hello, world!", id = id });
    }
}
