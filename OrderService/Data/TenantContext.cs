namespace OrderService.Data;

public class TenantContext : ITenantContext
{
    private string _connectionString;

    public string ConnectionString => _connectionString;
    public void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
    }
}