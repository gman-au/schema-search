using Microsoft.EntityFrameworkCore;

namespace SchemaSearch.EntityFramework
{
    public class SchemaDbContextFactory(DbContextOptions<SchemaDbContext> options) : IContextFactory
    {
        public SchemaDbContext GetContext()
        {
            return new SchemaDbContext(options);
        }
    }
}