using IKBExpenseDb.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(x => {
    x.UseSqlServer(builder.Configuration.GetConnectionString("Dev"));
});

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors(x => {
    x.AllowAnyOrigin()
     .AllowAnyHeader()
     .AllowAnyMethod();
});

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();
scope.ServiceProvider
    .GetService<AppDbContext>()!
    .Database.Migrate();

app.Run();
