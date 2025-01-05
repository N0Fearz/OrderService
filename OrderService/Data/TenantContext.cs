namespace OrderService.Data;

public class TenantContext : ITenantContext
{
    private string _connectionString;
    private readonly IConfiguration _configuration;

    public string ConnectionString => _connectionString;

    public TenantContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public void SetConnectionString(string schemaName)
    {
        var connectionString = _configuration.GetConnectionString("OrderDB");
        var connectionStringWithSchema = $"{connectionString}SearchPath={schemaName};";
        _connectionString = connectionStringWithSchema;
    }
}