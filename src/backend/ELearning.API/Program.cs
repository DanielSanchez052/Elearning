using ELearning.API.Extensions;
using ELearning.API.Middleware;
using ELearning.Application.DependencyInjection;
using ELearning.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCorsConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddDbContextConfiguration(builder.Configuration);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

app.UseStaticFiles();

app.UseExceptionHandling();
app.UseRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

app.UseCorsConfiguration();
app.UseSecurityHeaders();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ELearning.API.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
