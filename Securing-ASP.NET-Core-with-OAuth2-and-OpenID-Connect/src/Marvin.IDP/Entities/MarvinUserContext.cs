using Microsoft.EntityFrameworkCore;

namespace Marvin.IDP.Entities
{
    public class MarvinUserContext : DbContext
    {
        public MarvinUserContext(DbContextOptions<MarvinUserContext> options)
           : base(options)
        {
           
        }

        public DbSet<User> Users { get; set; }
    }
}
