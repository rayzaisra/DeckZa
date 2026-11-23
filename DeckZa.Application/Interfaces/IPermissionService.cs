using DeckZa.Application.Common;

namespace DeckZa.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasAccessAsync(PermissionAction action, params string[] menuUniques);
    Task<bool> HasScreenAccessAsync(string screenId, PermissionAction action);
}
