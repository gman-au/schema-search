using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SchemaSearch.Domain.Schema;

namespace SchemaSearch.Interfaces
{
    public interface ISchemaSearchApplication
    {
        Task<IEnumerable<SchemaTable>> RunAsync(CancellationToken cancellationToken = default);
    }
}