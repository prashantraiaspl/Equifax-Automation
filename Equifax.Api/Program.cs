using Equifax.Api.Data;
using Equifax.Api.Helper;
using Equifax.Api.Interfaces;
using Equifax.Api.Mappings;
using Equifax.Api.Repositories;
using Equifax.Api.Utilities;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File( path: "Logs/App-log.txt", outputTemplate: "---------- {Timestamp:yyyy-MM-dd HH:mm:ss} ----------{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", rollingInterval: RollingInterval.Day
        )
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddControllers();

builder.Services.AddAutoMapper(typeof(MappingProfile));


// Register ApplicationDbContext with the dependency injection system
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<DriverSetupManager>();
builder.Services.AddScoped<ElementLoader>();
builder.Services.AddScoped<BlocksLoader>();
builder.Services.AddScoped<BrowserUtility>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
