using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminAssistant.Data.Context;

public class AdminAssistantDbContext : DbContext
{
    public AdminAssistantDbContext(DbContextOptions<AdminAssistantDbContext> options)
        : base(options) { }

    public DbSet<AuditLogEntry> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PerformedBy).IsRequired().HasMaxLength(256);
            entity.Property(e => e.TargetUser).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ExecutedVia).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Action).HasConversion<string>(); // Enum als Text in DB
        });
    }
}
