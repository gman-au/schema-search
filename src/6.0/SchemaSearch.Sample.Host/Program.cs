using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchemaSearch.EntityFramework.Injection;
using SchemaSearch.Interfaces;

var host =
    Host
        .CreateDefaultBuilder(args)
        .ConfigureServices(
            (context, services) =>
            {
                services
                    .AddEntityFrameworkServices(context.Configuration);
            }
        )
        .Build();

using var scope =
    host
        .Services
        .CreateScope();

var searchApplication =
    scope
        .ServiceProvider
        .GetRequiredService<ISchemaSearchApplication>();

await
    searchApplication
        .RunAsync();