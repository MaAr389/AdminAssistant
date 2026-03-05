using AdminAssistant.Core.Interfaces;
using AdminAssistant.Core.Models;
using AdminAssistant.Data.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AdminAssistant.Data;

public class DatabaseMaintenanceService : IDatabaseMaintenanceService
{
    private readonly AdminAssistantDbContext _db;

    public DatabaseMaintenanceService(AdminAssistantDbContext db)
    {
        _db = db;
    }

    public async Task<DatabaseStatistics> GetStatisticsAsync()
    {
        //var dbPath = GetDatabasePath();
        var dbPath = await GetDatabasePathAsync();
        var info = new FileInfo(dbPath);

        var stats = new DatabaseStatistics
        {
            DatabasePath = dbPath,
            Exists = info.Exists,
            SizeBytes = info.Exists ? info.Length : 0,
            LastModifiedUtc = info.Exists ? info.LastWriteTimeUtc : null,
            TableRowCounts = new Dictionary<string, int>
            {
                ["AuditLogs"] = await _db.AuditLogs.CountAsync(),
                ["VpnSmartcardReaders"] = await _db.VpnSmartcardReaders.CountAsync(),
                ["VpnAccessCards"] = await _db.VpnAccessCards.CountAsync(),
                ["VpnInventorySettings"] = await _db.VpnInventorySettings.CountAsync()
            }
        };

        return stats;
    }

    public async Task<string> BackupAsync()
    {
        //var dbPath = GetDatabasePath();
        var dbPath = await GetDatabasePathAsync();
        if (!File.Exists(dbPath))
        {
            throw new FileNotFoundException("Datenbankdatei wurde nicht gefunden.", dbPath);
        }

        var backupDirectory = Path.Combine(AppContext.BaseDirectory, "Backups");
        Directory.CreateDirectory(backupDirectory);

        var fileName = $"adminassistant-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db";
        var backupPath = Path.Combine(backupDirectory, fileName);

        await _db.Database.CloseConnectionAsync();
        SqliteConnection.ClearAllPools();
        File.Copy(dbPath, backupPath, overwrite: false);

        return backupPath;
    }

    public async Task RestoreAsync(Stream backupStream, string sourceFileName)
    {
        if (!Path.GetExtension(sourceFileName).Equals(".db", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Es werden nur .db-Dateien unterstützt.");
        }

        //var dbPath = GetDatabasePath();
        var dbPath = await GetDatabasePathAsync();
        var dbDirectory = Path.GetDirectoryName(dbPath) ?? AppContext.BaseDirectory;
        Directory.CreateDirectory(dbDirectory);

        var tempRestorePath = Path.Combine(dbDirectory, $"restore-{Guid.NewGuid():N}.db");
        await using (var file = File.Create(tempRestorePath))
        {
            await backupStream.CopyToAsync(file);
        }

        var tempFileInfo = new FileInfo(tempRestorePath);
        if (!tempFileInfo.Exists || tempFileInfo.Length == 0)
        {
            File.Delete(tempRestorePath);
            throw new InvalidOperationException("Die hochgeladene Datei ist leer oder ungültig.");
        }

        await _db.Database.CloseConnectionAsync();
        SqliteConnection.ClearAllPools();

        if (File.Exists(dbPath))
        {
            var safetyBackupPath = Path.Combine(dbDirectory, $"pre-restore-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db");
            File.Copy(dbPath, safetyBackupPath, overwrite: false);
        }

        File.Copy(tempRestorePath, dbPath, overwrite: true);
        File.Delete(tempRestorePath);

        await _db.Database.MigrateAsync();
    }

    //private string GetDatabasePath()
    private async Task<string> GetDatabasePathAsync()
    {
        await _db.Database.OpenConnectionAsync();
        try
        {
                var connection = _db.Database.GetDbConnection();

            if (connection is SqliteConnection sqliteConnection)
            {
                await using var command = sqliteConnection.CreateCommand();
                command.CommandText = "PRAGMA database_list;";

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    // PRAGMA database_list columns: seq | name | file
                    var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    var file = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);

                    if (string.Equals(name, "main", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(file))
                    {
                        return Path.GetFullPath(file);
                    }
                }
            }

        var dataSource = connection.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource))
        {
            var connectionString = _db.Database.GetConnectionString();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var builder = new SqliteConnectionStringBuilder(connectionString);
                dataSource = builder.DataSource;
            }
        }

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            throw new InvalidOperationException("Datenbankpfad konnte nicht ermittelt werden.");
        }

        if (dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("In-Memory-Datenbank kann nicht gesichert oder zurückgespielt werden.");
        }

        if (Path.IsPathRooted(dataSource))
        {
            return Path.GetFullPath(dataSource);
        }

        var currentDirectoryPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), dataSource));
        var appBasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, dataSource));

            return File.Exists(currentDirectoryPath) ? currentDirectoryPath : appBasePath;
    }
        finally
        {
            await _db.Database.CloseConnectionAsync();
}

    }
}