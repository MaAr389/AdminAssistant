using AdminAssistant.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdminAssistant.Data;

public class AdminAssistantDbContextFactory : IDesignTimeDbContextFactory<AdminAssistantDbContext>
{
    public AdminAssistantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdminAssistantDbContext>();

        // Nur für Migrations / lokale Entwicklung
        optionsBuilder.UseSqlite("Data Source=adminassistant_dev.db");

        return new AdminAssistantDbContext(optionsBuilder.Options);
    }
}
