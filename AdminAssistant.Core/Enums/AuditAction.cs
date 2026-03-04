using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminAssistant.Core.Enums;

public enum AuditAction
{
    PasswordReset,
    AccountUnlock,      // vorbereitet für später
    AccountDisable,     // vorbereitet für später
    AccountEnable,      // vorbereitet für später
    GroupMemberAdd,     // vorbereitet für später
    GroupMemberRemove,   // vorbereitet für später
    UserAttributeUpdate
}
