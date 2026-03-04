using AdminAssistant.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminAssistant.Core.Models;

public class AuditLogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AuditAction Action { get; set; }
    public string PerformedBy { get; set; } = string.Empty; // KeyUser
    public string TargetUser { get; set; } = string.Empty; // Ziel-AD-User
    public string ExecutedVia { get; set; } = string.Empty; // t1_serviceuser
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IPAddress { get; set; }
    public string? Details { get; set; }
}
