using System.Collections.Generic;

namespace SchemaSearch.Domain.Schema
{
    public class SchemaTable
    {
        public string TableCatalog { get; set; }

        public string TableSchema { get; set; }

        public string TableName { get; set; }

        public string TableType { get; set; }

        public IEnumerable<SchemaTableColumn> Columns { get; set; }

        public IEnumerable<SchemaTableForeignKey> ForeignKeys { get; set; }

        public override string ToString()
        {
            return $"{TableSchema}.{TableName}";
        }
    }
}