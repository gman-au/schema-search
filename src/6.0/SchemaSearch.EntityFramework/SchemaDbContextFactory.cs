using Microsoft.EntityFrameworkCore;

namespace SchemaSearch.EntityFramework
{
    public class SchemaDbContextFactory : IContextFactory
    {
        private readonly DbContextOptions<SchemaDbContext> _options;

        public SchemaDbContextFactory(DbContextOptions<SchemaDbContext> options)
        {
            _options = options;
        }

        public SchemaDbContext GetContext()
        {
            return new SchemaDbContext(_options);
        }
    }
}