using System.Text.Json;
using ChatServer.API.Hubs;
using ChatServer.API.Middlewares;
using ChatServer.Application;
using ChatServer.Infrastructure;
using ChatServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowChatClient", policy =>
    {
        policy.WithOrigins(builder.Configuration["ChatClientIP"]!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Simple retry logic to wait for PostgreSQL
    var retries = 10;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception)
        {
            retries--;
            Console.WriteLine($"?? Waiting for PostgreSQL to be ready... Retries left: {retries}");
            Thread.Sleep(3000);
        }
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandlingMiddleware();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowChatClient");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/api/chat-hub");
app.Run();