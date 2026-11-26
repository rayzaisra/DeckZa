using DeckZa.Shared.Models;

namespace DeckZa.Shared.Services;

public class TabStateService
{
    public List<AppTabDto> Tabs { get; } = new();
    public AppTabDto? ActiveTab { get; private set; }
    public event Action? OnTabsChanged;

    public void OpenTab(
        string title,
        string? componentType,
        string? url,
        Dictionary<string, object>? parameters = null,
        string? icon = null,
        string? titleKey = null)
    {
        var existing = Tabs.FirstOrDefault(t => t.ComponentType == componentType && t.Url == url);
        if (existing != null)
        {
            if (parameters is { Count: > 0 })
            {
                existing.Parameters ??= new Dictionary<string, object>();
                foreach (var entry in parameters)
                {
                    existing.Parameters[entry.Key] = entry.Value;
                }

                existing.RefreshKey = Guid.NewGuid();
            }

            if (!string.IsNullOrWhiteSpace(titleKey))
            {
                existing.TitleKey = titleKey;
            }
            existing.Title = title;
            ActiveTab = existing;
            NotifyChanged();
            return;
        }

        var tab = new AppTabDto
        {
            Title = title,
            TitleKey = titleKey,
            ComponentType = componentType,
            Url = url,
            Icon = icon,
            Parameters = parameters,
        };

        Tabs.Add(tab);
        ActiveTab = tab;
        NotifyChanged();
    }

    public void ActivateTab(AppTabDto tab)
    {
        if (Tabs.Contains(tab))
        {
            ActiveTab = tab;
            NotifyChanged();
        }
    }

    public void CloseTab(AppTabDto tab)
    {
        if (!Tabs.Remove(tab))
        {
            return;
        }

        if (ActiveTab == tab)
        {
            ActiveTab = Tabs.LastOrDefault();
        }

        NotifyChanged();
    }

    public void RefreshTab(AppTabDto tab)
    {
        ActiveTab = tab;
        tab.RefreshKey = Guid.NewGuid();
        NotifyChanged();
    }

    public void CloseAll()
    {
        Tabs.Clear();
        ActiveTab = null;
        NotifyChanged();
    }

    private void NotifyChanged() => OnTabsChanged?.Invoke();
}
