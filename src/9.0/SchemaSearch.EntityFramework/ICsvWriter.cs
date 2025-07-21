using System.Threading;
using System.Threading.Tasks;

namespace SchemaSearch.EntityFramework
{
    public interface ICsvWriter
    {
        Task<byte[]> WriteToCsvAsync(string jsonData, CancellationToken cancellationToken = default);
    }
}