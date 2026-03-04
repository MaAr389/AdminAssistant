namespace AdminAssistant.Core.Interfaces;

public interface IOuAccessService
{
    bool CanAccessUser(string distinguishedName, bool isAdmin);
}
