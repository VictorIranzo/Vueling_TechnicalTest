namespace Microsoft.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Vueling.OTD.Persistence.Entities;

    public static class DbSetExtensions
    {
        public static void RemoveAllRows<T>(this DbSet<T> dbSet)
            where T : EntityBase, new()
        {
            dbSet.RemoveRange(
                dbSet.Select(e => new T() { Id = e.Id }));
        }
    }
}