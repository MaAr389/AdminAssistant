using AdminAssistant.Core.Interfaces;
using AdminAssistant.Core.Models;
using AdminAssistant.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminAssistant.Data;

public class AuditLogService : IAuditLogService
{
    private readonly AdminAssistantDbContext _db;

    public AuditLogService(AdminAssistantDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(AuditLogEntry entry)
    {
        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLogEntry>> GetLogsAsync(DateTime from, DateTime to)
    {
        return await _db.AuditLogs
            .Where(e => e.Timestamp >= from && e.Timestamp <= to)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }
}
