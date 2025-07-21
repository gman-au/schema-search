using System;
using System.Linq;
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

var allTables =
    (await
        searchApplication
            .RunAsync())
    .ToList();

Console
    .WriteLine($"Extracted {allTables.Count}");

foreach (var table in allTables)
{
    Console.WriteLine($"Table: {table}");
    foreach (var column in table.Columns)
    {
        Console.WriteLine($"\tColumn: {column}");
    }
    Console.WriteLine($"------");
}