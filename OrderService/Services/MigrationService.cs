using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public class MigrationService : IMigrationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogPublisher _logPublisher;
    public MigrationService(IServiceProvider serviceProvider, IConfiguration configuration, ILogPublisher logPublisher)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logPublisher = logPublisher;
    }

    public async Task AddSchemaAsync(string schemaName)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("OrderDB");
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseNpgsql(connectionString);


            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
                tenantContext.SetConnectionString(connectionString);
                var dbContext = new OrderDbContext(optionsBuilder.Options, scope.ServiceProvider.GetRequiredService<ITenantContext>());
                var cmd = new StringBuilder().Append("CREATE SCHEMA IF NOT EXISTS ").Append(schemaName).ToString();
                var formattableString = FormattableStringFactory.Create(cmd);
        
                await dbContext.Database.ExecuteSqlAsync(formattableString);
                
                _logPublisher.SendMessage(new LogMessage
                {
                    ServiceName = "OrderService",
                    LogLevel = "Information",
                    Message = "Schema created successfully.",
                    Timestamp = DateTime.Now,
                    Metadata = new Dictionary<string, string>
                    {
                        { "SchemaName", schemaName }
                    }
                });
            }
        }
        catch (Exception e)
        {
            _logPublisher.SendMessage(new LogMessage
            {
                ServiceName = "OrderService",
                LogLevel = "Error",
                Message = $"Failed to create schema {schemaName}. Error: {e.Message}",
                Timestamp = DateTime.Now,
                Metadata = new Dictionary<string, string>
                {
                    { "SchemaName", schemaName }
                }
            });
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task MigrateAsync(string schemaName)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("OrderDB");
            var connectionStringWithSchema = $"{connectionString}SearchPath={schemaName};";
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseNpgsql(connectionStringWithSchema);

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
                tenantContext.SetConnectionString(connectionStringWithSchema);
                var dbContext = new OrderDbContext(optionsBuilder.Options, scope.ServiceProvider.GetRequiredService<ITenantContext>());

                // Check if the migrations are needed
                if (await dbContext.Database.GetPendingMigrationsAsync() is { } migrations && migrations.Any())
                {
                    await dbContext.Database.MigrateAsync();
                    _logPublisher.SendMessage(new LogMessage
                    {
                        ServiceName = "OrderService",
                        LogLevel = "Information",
                        Message = "Migration completed successfully.",
                        Timestamp = DateTime.Now,
                        Metadata = new Dictionary<string, string>
                        {
                            { "SchemaName", schemaName }
                        }
                    });
                }
            }
        }
        catch (Exception e)
        {
            _logPublisher.SendMessage(new LogMessage
            {
                ServiceName = "OrderService",
                LogLevel = "Error",
                Message = $"Failed to migrate for schema {schemaName}. Error: {e.Message}",
                Timestamp = DateTime.Now,
                Metadata = new Dictionary<string, string>
                {
                    { "SchemaName", schemaName }
                }
            });
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task RemoveSchemaAsync(string schemaName)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("OrderDB");
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
                tenantContext.SetConnectionString(connectionString);
                var dbContext = new OrderDbContext(optionsBuilder.Options, scope.ServiceProvider.GetRequiredService<ITenantContext>());
        
                // Query to drop the schema
                var cmd = new StringBuilder().Append("DROP SCHEMA IF EXISTS ").Append(schemaName).Append(" CASCADE").ToString();
                var formattableString = FormattableStringFactory.Create(cmd);

                await dbContext.Database.ExecuteSqlAsync(formattableString);
                _logPublisher.SendMessage(new LogMessage
                {
                    ServiceName = "OrderService",
                    LogLevel = "Information",
                    Message = "Schema deleted successfully.",
                    Timestamp = DateTime.Now,
                    Metadata = new Dictionary<string, string>
                    {
                        { "SchemaName", schemaName }
                    }
                });
            }
        }
        catch (Exception e)
        {
            _logPublisher.SendMessage(new LogMessage
            {
                ServiceName = "OrderService",
                LogLevel = "Error",
                Message = $"Failed to delete schema {schemaName}. Error: {e.Message}",
                Timestamp = DateTime.Now,
                Metadata = new Dictionary<string, string>
                {
                    { "SchemaName", schemaName }
                }
            });
            Console.WriteLine(e);
            throw;
        }
    }
}