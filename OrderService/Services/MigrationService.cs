using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Services;

public class MigrationService : IMigrationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    public MigrationService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task MigrateAsync(string schemaName)
    {
        var connectionString = _configuration.GetConnectionString("ArticleDB");
        var connectionStringWithSchema = $"{connectionString+schemaName};";
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        optionsBuilder.UseNpgsql(connectionStringWithSchema);
        
        using var scope = _serviceProvider.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetConnectionString(connectionStringWithSchema);
        var dbContext = new OrderDbContext(optionsBuilder.Options, scope.ServiceProvider.GetRequiredService<ITenantContext>());

        // Check if the migrations are needed
        if (await dbContext.Database.GetPendingMigrationsAsync() is { } migrations && migrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
}