using Application;
using Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Clean Architecture services registration
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Background processing with Hangfire
builder.Services.AddHangfire(cfg =>
    cfg.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", p =>
        p.WithOrigins("http://localhost:3000")
         .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

// Hangfire Dashboard (internal tool UI)
app.UseHangfireDashboard("/hangfire");

app.MapControllers();
app.MapRazorPages();

app.Run();

