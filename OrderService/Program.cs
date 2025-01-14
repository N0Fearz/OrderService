using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Handlers;
using OrderService.Repository;
using OrderService.ServiceCollection;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var disableAuth = Environment.GetEnvironmentVariable("DISABLE_AUTH") == "true";

if (disableAuth)
{
    // Schakel Keycloak-authenticatie uit
    builder.Services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddKeycloakWebApi(builder.Configuration);
    
    builder.Services
        .AddAuthorization()
        .AddKeycloakAuthorization()
        .AddAuthorizationServer(builder.Configuration);
}

builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IMigrationService, MigrationService>();
builder.Services.AddSingleton<ITenantContext, TenantContext>();
builder.Services.AddSingleton<IOrderService, OrderService.Services.OrderService>();
builder.Services.AddHostedService<RabbitMQConsumer>();
builder.Services.AddHostedService<RabbitMqConsumeDeleteOrganization>();
builder.Services.AddSingleton<RabbitMqSenderOrganization>();
builder.Services.AddSingleton<ILogPublisher, LogPublisher>();
builder.Services.AddEndpointsApiExplorer().AddSwagger();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("OrderDB"),
        o => o
            .SetPostgresVersion(16, 4)));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowFrontend");
// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(options => options.EnableTryItOutByDefault());
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
await app.RunAsync();
