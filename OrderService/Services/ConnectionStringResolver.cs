namespace OrderService.Services;

public class ConnectionStringResolver: IConnectionStringResolver
{
    private readonly HttpClient _httpClient;

    public ConnectionStringResolver(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ResolveAsync(string organizationId)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            throw new ArgumentNullException(nameof(organizationId), "Organization ID cannot be null or empty.");
        }

        // API-aanroep naar de OrganizationService
        var response = await _httpClient.GetAsync($"/api/Org/{organizationId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}