using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SchemaSearch.Domain.Schema;

namespace SchemaSearch.Interfaces
{
    public interface ISchemaExtractor
    {
        Task<IEnumerable<SchemaTable>> PerformAsync(CancellationToken cancellationToken = default);
    }
}