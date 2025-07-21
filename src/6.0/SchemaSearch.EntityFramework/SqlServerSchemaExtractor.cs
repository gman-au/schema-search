using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SchemaSearch.Domain.Schema;
using SchemaSearch.Interfaces;

namespace SchemaSearch.EntityFramework
{
    public class SqlServerSchemaExtractor
        : ISchemaExtractor
    {
        private readonly IContextFactory _contextFactory;
        private readonly ILogger<SqlServerSchemaExtractor> _logger;

        public SqlServerSchemaExtractor(
            IContextFactory contextFactory,
            ILogger<SqlServerSchemaExtractor> logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger ?? NullLogger<SqlServerSchemaExtractor>.Instance;
        }

        public async Task<IEnumerable<SchemaTable>> PerformAsync(CancellationToken cancellationToken = default)
        {
            List<SchemaTable> tableResults;

            _logger
                .LogInformation("Attempting to extract schema");

            try
            {
                await using var db = _contextFactory.GetContext();

                var canConnect =
                    await
                        db
                            .Database
                            .CanConnectAsync(cancellationToken);

                if (canConnect)
                    _logger
                        .LogInformation("Connected to database");
                else
                    throw new Exception("Could not connect to database");

                tableResults =
                    (await
                        ExtractTablesAsync(cancellationToken)
                    ).ToList();

                foreach (var tableResult in tableResults)
                    tableResult.Columns =
                        await
                            ExtractTableColumnsAsync(tableResult, cancellationToken);

                foreach (var tableResult in tableResults)
                    tableResult.ForeignKeys =
                        await
                            ExtractTableForeignKeysAsync(tableResult, tableResults, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger
                    .LogError("Error extracting schema: {message}", ex.Message);

                throw;
            }

            return tableResults;
        }

        private async Task<IEnumerable<SchemaTable>> ExtractTablesAsync(CancellationToken cancellationToken)
        {
            List<SchemaTable> tableResults;

            try
            {
                await using var db = _contextFactory.GetContext();

                tableResults =
                    await
                        db
                            .SchemaTables
                            .FromSqlInterpolated(
                                $@"
                                 SELECT 
                                     TABLE_CATALOG AS TableCatalog,
                                     TABLE_SCHEMA AS TableSchema, 
                                     TABLE_NAME AS TableName, 
                                     TABLE_TYPE AS TableType 
                                 FROM INFORMATION_SCHEMA.TABLES
                                 ")
                            .ToListAsync(cancellationToken);

                _logger
                    .LogInformation("Extracted {count} tables", tableResults.Count);

                _logger
                    .LogDebug(
                        "Found tables:\r\n{tables}",
                        string.Join("\r\n", tableResults.Select(o => o.TableName))
                    );
            }
            catch (Exception ex)
            {
                _logger
                    .LogError("Error extracting schema tables: {message}", ex.Message);

                throw;
            }

            return tableResults;
        }

        private async Task<IEnumerable<SchemaTableColumn>> ExtractTableColumnsAsync(
            SchemaTable schemaTable,
            CancellationToken cancellationToken)
        {
            List<SchemaTableColumn> tableColumnResults;

            try
            {
                await using var db = _contextFactory.GetContext();

                var tableSchema = schemaTable.TableSchema;
                var tableName = schemaTable.TableName;

                tableColumnResults =
                    await
                        db
                            .SchemaTableColumns
                            .FromSqlInterpolated(
                                $@"
                                 SELECT 
                                     COLUMN_NAME AS ColumnName,
                                     ORDINAL_POSITION AS OrdinalPosition, 
                                     IIF(IS_NULLABLE = 'YES', CONVERT(BIT, 1), CONVERT(BIT, 0)) AS IsNullable,
                                     CHARACTER_MAXIMUM_LENGTH AS MaxLength, 
                                     CHARACTER_OCTET_LENGTH AS OctetLength, 
                                     NUMERIC_PRECISION AS NumericPrecision,
                                     CASE (DATA_TYPE)
                                        -- string
                                        WHEN 'char' THEN 1
                                        WHEN 'nvarchar' THEN 1
                                        WHEN 'varchar' THEN 1
                                        WHEN 'nchar' THEN 1
                                        WHEN 'text' THEN 1
                                        -- boolean
                                        WHEN 'bit' THEN 2
                                        -- integer
                                        WHEN 'tinyint' THEN 3
                                        WHEN 'smallint' THEN 3
                                        WHEN 'int' THEN 3
                                        WHEN 'bigint' THEN 3
                                        -- float
                                        WHEN 'decimal' THEN 4
                                        WHEN 'numeric' THEN 4
                                        WHEN 'money' THEN 4
                                        WHEN 'smallmoney' THEN 4
                                        WHEN 'float' THEN 4
                                        WHEN 'real' THEN 4
                                        -- datetime
                                        WHEN 'date' THEN 5
                                        WHEN 'time' THEN 5
                                        WHEN 'datetime2' THEN 5
                                        WHEN 'datetimeoffset' THEN 5
                                        WHEN 'datetime' THEN 5
                                        WHEN 'smalldatetime' THEN 5
                                        -- guid
                                        WHEN 'uniqueidentifier' THEN 6
                                        ELSE 0
                                     END as DataType
                                 FROM INFORMATION_SCHEMA.COLUMNS
                                 WHERE TABLE_SCHEMA = {tableSchema} 
                                 AND TABLE_NAME = {tableName}
                                 "
                            )
                            .ToListAsync(cancellationToken);

                _logger
                    .LogInformation(
                        "Extracted {count} columns from table {schema}.{table}",
                        tableColumnResults.Count,
                        tableSchema,
                        tableName
                    );
            }
            catch (Exception ex)
            {
                _logger
                    .LogError("Error extracting schema table columns for {schemaTable}: {message}", schemaTable,
                        ex.Message);

                throw;
            }

            return tableColumnResults;
        }

        private async Task<IEnumerable<SchemaTableForeignKey>> ExtractTableForeignKeysAsync(
            SchemaTable schemaTable,
            ICollection<SchemaTable> tables,
            CancellationToken cancellationToken)
        {
            List<SchemaTableForeignKey> tableForeignKeyResults;

            try
            {
                var unmatchedForeignKeys = 0;

                await using var db = _contextFactory.GetContext();

                var tableSchema = schemaTable.TableSchema;
                var tableName = schemaTable.TableName;

                tableForeignKeyResults =
                    await
                        db
                            .SchemaTableForeignKeys
                            .FromSqlInterpolated(
                                $@"
                                 SELECT 
                                      KCU1.CONSTRAINT_SCHEMA AS ForeignKeyConstraintSchema 
                                     ,KCU1.CONSTRAINT_NAME AS ForeignKeyConstraintName 
                                     ,KCU1.TABLE_SCHEMA AS ForeignKeyTableSchema 
                                     ,KCU1.TABLE_NAME AS ForeignKeyTableName
                                     ,KCU1.COLUMN_NAME AS ForeignKeyColumnName 
                                     ,KCU1.ORDINAL_POSITION AS ForeignKeyOrdinalPosition 
                                     ,KCU2.CONSTRAINT_SCHEMA AS ReferencedConstraintSchema 
                                     ,KCU2.CONSTRAINT_NAME AS ReferencedConstraintName 
                                     ,KCU2.TABLE_SCHEMA AS ReferencedTableSchema 
                                     ,KCU2.TABLE_NAME AS ReferencedTableName 
                                     ,KCU2.COLUMN_NAME AS ReferencedColumnName 
                                     ,KCU2.ORDINAL_POSITION AS ReferencedOrdinalPosition 
                                 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC
                                 INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU1 
                                     ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG  
                                     AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA 
                                     AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME 
                                 INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU2 
                                     ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG  
                                     AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA 
                                     AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME 
                                     AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION 
                                     WHERE KCU2.TABLE_SCHEMA = {tableSchema}
                                     AND KCU2.TABLE_NAME = {tableName}
                                 "
                            )
                            .ToListAsync(cancellationToken);

                foreach (var tableForeignKeyResult in tableForeignKeyResults)
                {
                    var foreignKeyTable =
                        tables
                            .FirstOrDefault(
                                t => t.TableSchema == tableForeignKeyResult.ForeignKeyTableSchema &&
                                     t.TableName == tableForeignKeyResult.ForeignKeyTableName);

                    var foreignKeyColumn =
                        foreignKeyTable?
                            .Columns?
                            .FirstOrDefault(
                                c => c.ColumnName == tableForeignKeyResult.ForeignKeyColumnName &&
                                     c.OrdinalPosition == tableForeignKeyResult.ForeignKeyOrdinalPosition);

                    var referencedTable =
                        tables
                            .FirstOrDefault(
                                t => t.TableSchema == tableForeignKeyResult.ReferencedTableSchema &&
                                     t.TableName == tableForeignKeyResult.ReferencedTableName);

                    var referencedColumn =
                        referencedTable?
                            .Columns?
                            .FirstOrDefault(
                                c => c.ColumnName == tableForeignKeyResult.ReferencedColumnName &&
                                     c.OrdinalPosition == tableForeignKeyResult.ReferencedOrdinalPosition);

                    // If not matched, warn and nullify
                    if (referencedColumn == null || foreignKeyColumn == null)
                    {
                        _logger
                            .LogWarning(
                                @"
                                Could not match foreign key constraint {0}.{1} 
                                to referenced constraint {2}.{3}
                                ",
                                tableForeignKeyResult.ForeignKeyConstraintSchema,
                                tableForeignKeyResult.ForeignKeyConstraintName,
                                tableForeignKeyResult.ReferencedConstraintSchema,
                                tableForeignKeyResult.ReferencedConstraintName
                            );
                        unmatchedForeignKeys++;
                    }
                    else
                    {
                        tableForeignKeyResult.ForeignKeyColumn = foreignKeyColumn;
                        tableForeignKeyResult.ReferencedColumn = referencedColumn;
                    }
                }

                _logger
                    .LogInformation(
                        "Extracted {count} foreign keys from table {schemaTable}, {unmatched} unmatched",
                        tableForeignKeyResults.Count,
                        schemaTable,
                        unmatchedForeignKeys
                    );
            }
            catch (Exception ex)
            {
                _logger
                    .LogError("Error extracting foreign keys for {schemaTable}: {message}", schemaTable, ex.Message);

                throw;
            }

            return tableForeignKeyResults;
        }
    }
}