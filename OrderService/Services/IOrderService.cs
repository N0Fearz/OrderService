namespace OrderService.Services;

public interface IOrderService
{
    public Task<string> GetTenantSchemaName(string token);
    public void SetConnectionString(string schemaName);
}