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
builder.Services.AddDbContextConfiguration(builder.Configuration);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();


app.UseExceptionHandling();
app.UseRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCorsConfiguration();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ELearning.API.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
