using Microsoft.EntityFrameworkCore;

namespace ConcurrencyTestingEFCore.Models
{
    public class AppDataContext : DbContext
    {
        public DbSet<TestItem> TestItems { get; set; }

        public AppDataContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ConcurrencyTestingEFCoreDb;Trusted_Connection=True;MultipleActiveResultSets=true");

            #region EnableRetryOnFailure options

            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ConcurrencyTestingEFCoreDb;Trusted_Connection=True;MultipleActiveResultSets=true",
                options => options.EnableRetryOnFailure());

            //optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ConcurrencyTestingEFCoreDb;Trusted_Connection=True;MultipleActiveResultSets=true",
            //    sqlServerOptionsAction: sqlOptions =>
            //     {
            //         sqlOptions.EnableRetryOnFailure(maxRetryCount: 5,
            //             maxRetryDelay: TimeSpan.FromSeconds(30),
            //             errorNumbersToAdd: null);
            //     });

            #endregion

        }
    }
}
