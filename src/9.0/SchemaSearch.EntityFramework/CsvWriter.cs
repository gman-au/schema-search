using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SchemaSearch.EntityFramework
{
    public class CsvWriter : ICsvWriter
    {
        public async Task<byte[]> WriteToCsvAsync(string jsonData, CancellationToken cancellationToken = default)
        {
            using var document = JsonDocument.Parse(jsonData);
            var csv = new StringBuilder();

            if (document.RootElement.GetArrayLength() > 0)
            {
                // Get headers from first object
                var firstElement =
                    document
                        .RootElement[0];

                var headers =
                    firstElement
                        .EnumerateObject()
                        .Select(p => p.Name)
                        .ToList();

                csv
                    .AppendLine(string.Join(",", headers));

                // Add data rows
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    var values = headers.Select(header =>
                    {
                        if (element.TryGetProperty(header, out var prop))
                        {
                            var value = prop.ToString();
                            // Escape commas and quotes
                            if (value.Contains(",") || value.Contains("\""))
                                return $"\"{value.Replace("\"", "\"\"")}\"";
                            return value;
                        }
                        return "";
                    });
                    csv.AppendLine(string.Join(",", values));
                }
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());

            return bytes;
        }
    }
}