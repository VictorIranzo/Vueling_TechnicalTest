namespace Vueling.OTD.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using Vueling.OTD.Persistence.Entities;

    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Rate> Rates { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
    }
}