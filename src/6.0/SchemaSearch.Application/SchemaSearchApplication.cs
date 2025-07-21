using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SchemaSearch.Domain.Schema;
using SchemaSearch.Interfaces;

namespace SchemaSearch.Application
{
    public class SchemaSearchApplication
        : ISchemaSearchApplication
    {
        private readonly ISchemaExtractor _schemaExtractor;
        private readonly ILogger<SchemaSearchApplication> _logger;

        public SchemaSearchApplication(
            ISchemaExtractor schemaExtractor,
            ILogger<SchemaSearchApplication> logger = null
            )
        {
            _schemaExtractor = schemaExtractor;
            _logger = logger ?? NullLogger<SchemaSearchApplication>.Instance;
        }

        public async Task<IEnumerable<SchemaTable>> RunAsync(CancellationToken cancellationToken = default)
        {
            _logger?
                .LogInformation("Running schema extraction");

            var tables =
                await
                    _schemaExtractor
                        .PerformAsync(cancellationToken);

            return tables;
        }
    }
}