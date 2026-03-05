using ArenaBackend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public interface ITabsService
    {
        Task<IEnumerable<object>> GetTabsAsync(string? status = null);
        Task<object?> GetTabAsync(int id);
        Task<object> OpenTabAsync(CreateTabDto dto);
        Task<string> AddItemAsync(int id, AddTabItemDto dto);
        Task<string> RemoveItemAsync(int id, int itemId);
        Task<object> CloseTabAsync(int id, CloseTabDto dto);
    }
}
