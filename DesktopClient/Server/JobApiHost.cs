using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DesktopClient.Server
{
    public class JobApiHost
    {
        private readonly ServerThread _server;
        private readonly int _port;
        private IHost? _host;

        public JobApiHost(ServerThread server, int port)
        {
            _server = server;
            _port = port;
        }

        public void Start()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls($"http://*:{_port}");

            var app = builder.Build();

            app.MapGet("/getjob", () =>
            {
                var job = _server.GetNextJob();
                return job is not null ? Results.Ok(job) : Results.NoContent();
            });

            app.MapPost("/submit", async (HttpRequest request) =>
            {
                var job = request.Query["job"];
                var result = await request.ReadFromJsonAsync<string>();
                // You can extend this to store results, notify the GUI, etc.
                Console.WriteLine($"Received solution for job: {job}\nResult: {result}");
                return Results.Ok();
            });

            app.Start();
            _host = app;
        }

        public async Task StopAsync()
        {
            if (_host != null)
                await _host.StopAsync();
        }
    }
}