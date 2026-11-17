using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeckZa.Infrastructure.Data;

public class DeckZaDbContextFactory : IDesignTimeDbContextFactory<DeckZaDbContext>
{
    public DeckZaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DeckZaDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=deckza_db;Integrated Security=True;TrustServerCertificate=True;Encrypt=False")
            .Options;

        return new DeckZaDbContext(options);
    }
}

public class AuditLogDbContextFactory : IDesignTimeDbContextFactory<AuditLogDbContext>
{
    public AuditLogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuditLogDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=deckza_log_db;Integrated Security=True;TrustServerCertificate=True;Encrypt=False")
            .Options;

        return new AuditLogDbContext(options);
    }
}
