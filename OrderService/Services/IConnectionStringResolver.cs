namespace OrderService.Services;

public interface IConnectionStringResolver
{
    Task<string> ResolveAsync(string organizationId);
}