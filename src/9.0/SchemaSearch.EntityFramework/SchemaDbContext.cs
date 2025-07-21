using Microsoft.EntityFrameworkCore;
using SchemaSearch.Domain.Schema;

namespace SchemaSearch.EntityFramework
{
    public class SchemaDbContext(DbContextOptions<SchemaDbContext> options) : DbContext(options)
    {
        public virtual DbSet<SchemaTable> SchemaTables { get; set; }

        public virtual DbSet<SchemaTableColumn> SchemaTableColumns { get; set; }

        public virtual DbSet<SchemaTableForeignKey> SchemaTableForeignKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<SchemaTable>()
                .HasNoKey();

            modelBuilder
                .Entity<SchemaTableColumn>()
                .HasNoKey();

            modelBuilder
                .Entity<SchemaTableForeignKey>()
                .HasNoKey();

            modelBuilder
                .Entity<SchemaTableForeignKey>()
                .Ignore(t => t.ForeignKeyColumn)
                .Ignore(t => t.ForeignKeyTable)
                .Ignore(t => t.ReferencedColumn)
                .Ignore(t => t.ReferencedTable);
        }
    }
}