namespace OrderService.Services;

public interface IMigrationService
{
    Task MigrateAsync(string schemaName);
    Task AddSchemaAsync(string schemaName);
}