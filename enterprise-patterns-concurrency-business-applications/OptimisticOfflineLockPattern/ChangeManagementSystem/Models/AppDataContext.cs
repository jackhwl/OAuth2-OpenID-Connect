using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public class AppDataContext : IdentityDbContext<IdentityUser>
    {
        public AppDataContext(DbContextOptions<AppDataContext> options) : base(options)
        {
        }

        public DbSet<ChangeRequest> ChangeRequests { get; set; }
        public DbSet<ChangeRequestTask> ChangeRequestTasks { get; set; }
        public DbSet<Lock> Locks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Lock>().HasKey(table => new {table.EntityId, table.EntityName});
            base.OnModelCreating(builder);
        }
    }
}
