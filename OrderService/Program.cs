using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Repository;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddHttpClient<IConnectionStringResolver, ConnectionStringResolver>(client =>
{
    client.BaseAddress = new Uri("localhost:5000");
});
//var serviceProvider = builder.Services.BuildServiceProvider();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddKeycloakWebApi(builder.Configuration,
        options =>
            options.Events.OnTokenValidated = async ctx =>
            {
                // For some reason, the access token's claims are not getting added to the user in C#
                // So this method hooks into the TokenValidation and adds it manually...
                // This definitely seems like a bug to me.
                // First, let's just get the access token and read it as a JWT
                var token = ctx.SecurityToken;
                var handler = new JwtSecurityTokenHandler();
                var Jwt = handler.WriteToken(token);
                var parsedJwt = handler.ReadJwtToken(Jwt);
                var org = parsedJwt.Claims.First(c => c.Type == "organization").Value;


                builder.Services.AddDbContextPool<OrderDbContext>((serviceProvider, opt) =>
                {
                    var resolver = serviceProvider.GetRequiredService<IConnectionStringResolver>();
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

                    var organizationId = httpContext?.Items["organization"]?.ToString();
                    if (organizationId == null)
                    {
                        throw new UnauthorizedAccessException("OrganizationId is missing.");
                    }

                    // Ophalen van de connectionstring via de resolver
                    var connectionString = resolver.ResolveAsync(organizationId).Result;
                    opt.UseNpgsql(connectionString);
                });
            }
    );

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
