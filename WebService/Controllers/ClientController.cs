using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebService.DataAccessLayer;
using WebService.Models;

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ClientController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _context.Clients.ToListAsync();
            return Ok(clients);
        }

        [HttpPost]

        public async Task<IActionResult> RegisterClient([FromBody] ClientInfo clientInfo)
        {
            if (clientInfo == null)
            {
                return BadRequest("Client information is required.");
            }
            _context.Clients.Add(clientInfo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetClients), new { id = clientInfo.Id }, clientInfo);
        }

        [HttpPost("jobdone")]

        public async Task<IActionResult> JobDone([FromBody] JobDoneRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.IPAddress) || request.Port == 0)
                return BadRequest("Invalid request.");

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.IPAddress == request.IPAddress && c.Port == request.Port);

            if (client == null)
                return NotFound("Client not found.");

            client.JobsCompleted++;
            await _context.SaveChangesAsync();

            return Ok(client);
        }

        public class JobDoneRequest
        {
            public required string IPAddress { get; set; }
            public int Port { get; set; }
        }

    }
}
