using solver;
using ImageMagick;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

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
    const string Root = @"C:\Users\cashto\Documents\GitHub\icfp2022";

    class CutOption
    {
        public CutOption(string name, Rectangle new_rect, string orientation, int line_number, int keep)
        {
            this.name = name;
            this.new_rect = new_rect;
            this.orientation = orientation;
            this.line_number = line_number;
            this.keep = keep;
        }

        public string name { get; set; }
        public Rectangle new_rect { get; set; }
        public string orientation { get; set; }
        public int line_number { get; set; }
        public int keep { get; set; }
    }

    public class SubmitBody
    {
        public List<Rectangle> rects { get; set; }
    }

    [HttpPost]
    [Route("save/{id}")]
    public IActionResult Save(int id, [FromBody] SubmitBody body)
    {
        var filename = DateTime.UtcNow.ToString("O").Replace(":", "-");
        Directory.CreateDirectory($"{Root}\\work\\save\\{id}");
        System.IO.File.WriteAllText($"{Root}\\work\\save\\{id}\\{filename}.json", JsonConvert.SerializeObject(body));
        System.IO.File.WriteAllText($"{Root}\\work\\save\\{id}\\current.json", JsonConvert.SerializeObject(body));
        return Ok();
    }

    [HttpGet]
    [Route("load/{id}")]
    public IActionResult Load(int id)
    {
        try
        {
            return Ok(System.IO.File.ReadAllText($"{Root}\\work\\save\\{id}\\current.json"));
        }
        catch
        {
            return Ok(JsonConvert.SerializeObject(new SubmitBody() { rects = new List<Rectangle>() }));
        }
    }

    IEnumerable<string> GenerateISL(int id, SubmitBody body)
    {
        var current_node_id = 0;
        string reset = null;

        switch (id)
        {
            case 27:
            case 30:
                reset = "great-reset";
                current_node_id = 798;
                break;

            case 31:
            case 32:
            case 33:
            case 34:
            case 35:
                reset = "medium-reset";
                current_node_id = 510;
                break;

            case 26:
            case 28:
            case 29:
                reset = "small-reset";
                current_node_id = 198;
                break;
        }

        if (reset != null)
        {
            foreach (var line in System.IO.File.ReadAllLines($"{Root}\\work\\{reset}.txt"))
            {
                yield return line;
            }
        }

        var image = solver.Program.Image.Load($"{Root}\\work\\problems\\{id}.png");

        var rectColors = solver.Program.GetRectangleColors(image, body.rects.Reverse<Rectangle>(), true).Reverse<Int32[]>().ToList();

        string last_name = null;

        foreach (var target_idx in Enumerable.Range(0, body.rects.Count))
        {
            var target = body.rects[target_idx];
            var not_taken = new List<string>();

            var work = new Rectangle() { x = 0, y = 0, dx = 400, dy = 400, name = current_node_id.ToString() };
            while (!work.Equals(target))
            {
                var left = target.x - work.x;
                var top = target.y - work.y;
                var right = work.x + work.dx - target.x - target.dx;
                var bottom = work.y + work.dy - target.y - target.dy;

                var options = new List<CutOption>()
                {
                    new CutOption("left", new Rectangle(target.x, work.y, work.dx - left, work.dy), "x", target.x, 1),
                    new CutOption("top", new Rectangle(work.x, target.y, work.dx, work.dy - top), "y", target.y, 0),
                    new CutOption("right", new Rectangle(work.x, work.y, work.dx - right, work.dy), "x", target.x + target.dx, 0),
                    new CutOption("bottom", new Rectangle(work.x, work.y, work.dx, work.dy - bottom), "y", target.y + target.dy, 1)
                };

                var sorted_options =
                    from option in options
                    where option.new_rect.dx * option.new_rect.dy != work.dx * work.dy
                    orderby option.new_rect.dx * option.new_rect.dy descending
                    select option;

                var best_option = sorted_options.First();

                var orientation = best_option.orientation;
                var line_number = best_option.line_number;
                if (orientation == "y")
                {
                    line_number = 400 - line_number;
                }

                yield return $"cut [{work.name}] [{orientation}] [{line_number}]";
                var new_name = $"{work.name}.{best_option.keep}";
                not_taken.Add($"{work.name}.{1 - best_option.keep}");
                work = best_option.new_rect;
                work.name = new_name;
                last_name = new_name;
            }

            var color = rectColors[target_idx + 1];

            yield return $"color [{work.name}] [{color[0]}, {color[1]}, {color[2]}, {color[3]}]";

            foreach (var i in not_taken.Reverse<string>())
            {
                yield return $"merge [{i}] [{last_name}]";
                ++current_node_id;
                last_name = current_node_id.ToString();
            }
        }
    }

    [HttpPost]
    [Route("submit/{id}")]
    public IActionResult Submit(int id, [FromBody] SubmitBody body)
    {
        var dirname = DateTime.UtcNow.ToString("O").Replace(":", "-");
        Directory.CreateDirectory($"{Root}\\work\\submissions\\{id}\\{dirname}");
        System.IO.File.WriteAllLines($"{Root}\\work\\submissions\\{id}\\{dirname}\\request.txt", GenerateISL(id, body));
        using (var process = new Process())
        {
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = $"{Root}\\work\\submit.cmd";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = $"{id} {dirname}";
            process.Start();
        }

        return Ok();
    }
}
