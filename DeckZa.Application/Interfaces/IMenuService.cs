using DeckZa.Application.DTOs.Menu;

namespace DeckZa.Application.Interfaces;

public interface IMenuService
{
    Task<List<MenuItemDto>> GetMenuTreeByGroupIdAsync(string groupId);
}
