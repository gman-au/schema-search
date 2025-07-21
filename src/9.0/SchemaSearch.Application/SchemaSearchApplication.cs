using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SchemaSearch.Domain.Schema;
using SchemaSearch.Interfaces;

namespace SchemaSearch.Application
{
    public class SchemaSearchApplication(
        ILogger<SchemaSearchApplication> logger,
        ISchemaExtractor schemaExtractor)
        : ISchemaSearchApplication
    {
        public async Task<IEnumerable<SchemaTable>> RunAsync(CancellationToken cancellationToken = default)
        {
            logger
                .LogInformation("Running schema extraction");

            var tables =
                await
                    schemaExtractor
                        .PerformAsync(cancellationToken);

            return tables;
        }
    }
}