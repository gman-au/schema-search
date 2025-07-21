namespace SchemaSearch.Domain.Schema
{
    public class SchemaTableForeignKey
    {
        public string ForeignKeyConstraintSchema { get; set; }

        public string ForeignKeyConstraintName { get; set; }

        public string ForeignKeyTableSchema { get; set; }

        public string ForeignKeyTableName { get; set; }

        public string ForeignKeyColumnName { get; set; }

        public int ForeignKeyOrdinalPosition { get; set; }

        public SchemaTable ForeignKeyTable { get; set; }

        public SchemaTableColumn ForeignKeyColumn { get; set; }

        public string ReferencedConstraintSchema { get; set; }

        public string ReferencedTableSchema { get; set; }

        public string ReferencedTableName { get; set; }

        public string ReferencedConstraintName { get; set; }

        public string ReferencedColumnName { get; set; }

        public int ReferencedOrdinalPosition { get; set; }

        public SchemaTable ReferencedTable { get; set; }

        public SchemaTableColumn ReferencedColumn { get; set; }

        public override string ToString()
        {
            return $"{ForeignKeyConstraintSchema}.{ForeignKeyConstraintName}";
        }
    }
}