using ArenaBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArenaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        public AnalyticsController(IAnalyticsService analyticsService) { _analyticsService = analyticsService; }

        // GET: api/analytics/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _analyticsService.GetDashboardAsync();
            return Ok(data);
        }

        // GET: api/analytics/sales-by-day?days=30
        [HttpGet("sales-by-day")]
        public async Task<IActionResult> GetSalesByDay([FromQuery] int days = 30)
        {
            var data = await _analyticsService.GetSalesByDayAsync(days);
            return Ok(data);
        }

        // GET: api/analytics/sales-by-category
        [HttpGet("sales-by-category")]
        public async Task<IActionResult> GetSalesByCategory()
        {
            var data = await _analyticsService.GetSalesByCategoryAsync();
            return Ok(data);
        }

        // GET: api/analytics/top-products?limit=10
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 10)
        {
            var data = await _analyticsService.GetTopProductsAsync(limit);
            return Ok(data);
        }

        // GET: api/analytics/sales-by-product?days=30
        [HttpGet("sales-by-product")]
        public async Task<IActionResult> GetSalesByProduct([FromQuery] int days = 30)
        {
            var data = await _analyticsService.GetSalesByProductAsync(days);
            return Ok(data);
        }

        // GET: api/analytics/inventory-summary
        [HttpGet("inventory-summary")]
        public async Task<IActionResult> GetInventorySummary()
        {
            var data = await _analyticsService.GetInventorySummaryAsync();
            return Ok(data);
        }
    }
}
