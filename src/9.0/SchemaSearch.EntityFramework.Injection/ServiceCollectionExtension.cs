using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchemaSearch.Application;
using SchemaSearch.Interfaces;

namespace SchemaSearch.EntityFramework.Injection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSqlServerServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddTransient<ISchemaExtractor, SqlServerSchemaExtractor>()
                .AddTransient<ISchemaSearchApplication, SchemaSearchApplication>();

            services
                .AddTransient<IContextFactory, SchemaDbContextFactory>();

            var connectionString =
                configuration
                    .GetConnectionString("Default") ??
                throw new Exception("Connection string not found or defined");

            services
                .AddDbContext<SchemaDbContext>(options =>
                    options
                        .UseSqlServer(connectionString));

            return services;
        }
    }
}