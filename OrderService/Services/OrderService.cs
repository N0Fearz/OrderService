using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using OrderService.Data;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly RabbitMqSenderOrganization _rabbitMqSenderOrganization;
    private readonly ITenantContext _tenantContext;
    public OrderService(RabbitMqSenderOrganization senderOrganization, ITenantContext tenantContext)
    {
        _rabbitMqSenderOrganization = senderOrganization;
        _tenantContext = tenantContext;
        
    }

    public async Task<string> GetTenantSchemaName(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var parsedJwt = handler.ReadJwtToken(token);
        var org = parsedJwt.Claims.First(c => c.Type == "organization").Value;

        var parsed = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(org);
        var id = parsed?.Values.FirstOrDefault()?.GetValueOrDefault("id");
        var message = await _rabbitMqSenderOrganization.SendMessage(id);

        return message;
    }

    public void SetConnectionString(string schemaName)
    {
        _tenantContext.SetConnectionString(schemaName);
    }
}