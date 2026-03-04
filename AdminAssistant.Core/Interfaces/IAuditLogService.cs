using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminAssistant.Core.Models;

namespace AdminAssistant.Core.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(AuditLogEntry entry);
    Task<IEnumerable<AuditLogEntry>> GetLogsAsync(DateTime from, DateTime to);
}
