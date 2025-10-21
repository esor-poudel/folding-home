using Microsoft.EntityFrameworkCore;
using WebService.Models;

namespace WebService.DataAccessLayer
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<ClientInfo> Clients { get; set; }

       
        
    }
}
