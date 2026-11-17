using DeckZa.Application.DTOs.Menu;
using DeckZa.Application.Interfaces;
using DeckZa.Application.Interfaces.Queries;

namespace DeckZa.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly IMenuQueryService _queryService;

    public MenuService(IMenuQueryService queryService) => _queryService = queryService;

    public Task<List<MenuItemDto>> GetMenuTreeByGroupIdAsync(string groupId)
        => _queryService.GetMenuTreeByGroupIdAsync(groupId);
}
