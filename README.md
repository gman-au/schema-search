# SchemaSearch

[![nuget](https://github.com/gman-au/schema-search/actions/workflows/nuget.yml/badge.svg)](https://github.com/gman-au/schema-search/actions/workflows/nuget.yml)

![GitHub Release](https://img.shields.io/github/v/release/gman-au/schema-search)

## Summary
The SchemaSearch application is a class library that will build a set of database tables, columns and foreign key relationships 
when supplied with a valid database connection string.

A sample console host is included. To test it, create or connect to a database 
(you can run a sample [Northwind script](https://github.com/microsoft/sql-server-samples/blob/master/samples/databases/northwind-pubs/instnwnd.sql) to create a test database)
and supply the connection string:

* either as an environment variable
```
ConnectionStrings__Default=<YOUR_CONNECTION_STRING>
```
* or as part of the `appsettings.json` configuration:
```json
{
    "ConnectionStrings": {
        "Default": "<YOUR_CONNECTION_STRING>"
    }
}
```

## Usage
The Entity Framework implementation can be injected in your program startup via the following services extension function:
```csharp
services
    .AddEntityFrameworkServices(configuration);
```
Where `configuration` is an IConfiguration object with valid connection string data.

From here, you can then make the call to retrieve the schema tables via the `ISchemaSearchApplication` interface:
```csharp
var allTables =
    (await
        searchApplication
            .RunAsync())
    .ToList();
```