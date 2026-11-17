using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DeckZa.Infrastructure.Data;

public class DeckZaDbContextFactory : IDesignTimeDbContextFactory<DeckZaDbContext>
{
    public DeckZaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DeckZaDbContext>()
            .UseSqlServer(DesignTimeConnection.Resolve("DefaultConnection"))
            .Options;

        return new DeckZaDbContext(options);
    }
}

public class AuditLogDbContextFactory : IDesignTimeDbContextFactory<AuditLogDbContext>
{
    public AuditLogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuditLogDbContext>()
            .UseSqlServer(DesignTimeConnection.Resolve("AuditLogConnection"))
            .Options;

        return new AuditLogDbContext(options);
    }
}

internal static class DesignTimeConnection
{
    public static string Resolve(string name)
    {
        var fromEnvironment = Environment.GetEnvironmentVariable($"ConnectionStrings__{name}");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";
        var root = FindSolutionRoot();

        foreach (var directory in new[] { root }.Concat(Directory.GetDirectories(root)))
        {
            if (!File.Exists(Path.Combine(directory, "appsettings.json")))
            {
                continue;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString(name);
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }
        }

        throw new InvalidOperationException(
            $"Connection string '{name}' was not found in environment variables or appsettings files.");
    }

    private static string FindSolutionRoot()
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
             directory != null;
             directory = directory.Parent)
        {
            if (directory.GetFiles("*.sln").Length > 0)
            {
                return directory.FullName;
            }
        }

        return Directory.GetCurrentDirectory();
    }
}
