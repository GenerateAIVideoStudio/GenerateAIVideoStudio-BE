using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var apiPath = Path.Combine(basePath, "src", "Api");
        if (Directory.Exists(apiPath))
        {
            basePath = apiPath;
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=ugcads;Username=postgres;Password=12345;Port=5432";

        builder.UseNpgsql(connectionString)
               .UseSnakeCaseNamingConvention();

        return new AppDbContext(builder.Options);
    }
}
