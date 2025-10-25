using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Dashboard.Models;

namespace Dashboard.Controllers
{
    public class DashboardController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var apiUrl = "https://localhost:7194/api/client"; 
            List<ClientInfo> clients = new();
            using var http = new HttpClient();
            try
            {
                var result = await http.GetFromJsonAsync<List<ClientInfo>>(apiUrl);
                if (result != null)
                    clients = result;
            }
            catch
            {
                // Handle error (e.g., log or show message)
            }
            return View(clients);
        }
    }

    public class ClientInfo
    {
        public int Id { get; set; }
        public required string IPAddress { get; set; }
        public int Port { get; set; }
        public string? DisplayName { get; set; }
        public int JobsCompleted { get; set; }
        public System.DateTime RegisteredAt { get; set; }
    }


}
