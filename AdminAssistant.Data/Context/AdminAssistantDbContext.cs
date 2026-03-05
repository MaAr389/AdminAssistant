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
    public DbSet<VpnSmartcardReader> VpnSmartcardReaders { get; set; }
    public DbSet<VpnAccessCard> VpnAccessCards { get; set; }
    public DbSet<VpnInventorySettings> VpnInventorySettings { get; set; }
    public DbSet<OuPermission> OuPermissions { get; set; }

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
        modelBuilder.Entity<VpnSmartcardReader>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AssignedAdUser).HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.HasIndex(e => e.SerialNumber).IsUnique();
        });


        modelBuilder.Entity<VpnInventorySettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalLicenses).IsRequired();
            entity.HasData(new VpnInventorySettings { Id = 1, TotalLicenses = 150 });
        });


        modelBuilder.Entity<OuPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Area).IsRequired().HasMaxLength(64);
            entity.Property(e => e.DistinguishedName).IsRequired().HasMaxLength(1024);
            entity.HasIndex(e => new { e.Area, e.DistinguishedName }).IsUnique();
        });

        modelBuilder.Entity<VpnAccessCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CardNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AssignedAdUser).HasMaxLength(256);
            entity.Property(e => e.Pin).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(512);
            entity.HasIndex(e => e.CardNumber).IsUnique();
        });
    }
}
