namespace SchemaSearch.EntityFramework
{
    public interface IContextFactory
    {
        public SchemaDbContext GetContext();
    }
}