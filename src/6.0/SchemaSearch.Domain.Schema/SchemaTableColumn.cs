using SchemaSearch.Domain.Schema.Enum;

namespace SchemaSearch.Domain.Schema
{
    public class SchemaTableColumn
    {
        public string ColumnName { get; set; }

        public int OrdinalPosition { get; set; }

        public bool IsNullable { get; set; }

        public string DataType { get; set; }

        public DataTypeEnum MappedDataType { get; set; }

        public int? MaxLength { get; set; }

        public int? OctetLength { get; set; }

        public byte? NumericPrecision { get; set; }

        public override string ToString()
        {
            return $"{ColumnName} [{DataType}]";
        }
    }
}