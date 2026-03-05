using AdminAssistant.Core.Models;

namespace AdminAssistant.Core.Interfaces;

public interface IDatabaseMaintenanceService
{
    Task<DatabaseStatistics> GetStatisticsAsync();
    Task<string> BackupAsync();
    Task RestoreAsync(Stream backupStream, string sourceFileName);
}
