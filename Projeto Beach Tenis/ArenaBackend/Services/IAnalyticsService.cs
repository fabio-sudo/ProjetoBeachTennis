using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public interface IAnalyticsService
    {
        Task<object> GetDashboardAsync();
        Task<IEnumerable<object>> GetSalesByDayAsync(int days);
        Task<IEnumerable<object>> GetSalesByCategoryAsync();
        Task<IEnumerable<object>> GetTopProductsAsync(int limit);
        Task<IEnumerable<object>> GetSalesByProductAsync(int days);
        Task<IEnumerable<object>> GetInventorySummaryAsync();
    }
}
