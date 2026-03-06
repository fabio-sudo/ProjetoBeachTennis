using ArenaBackend.DTOs;
using ArenaBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArenaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TabsController : ControllerBase
    {
        private readonly ITabsService _tabsService;
        public TabsController(ITabsService tabsService) { _tabsService = tabsService; }

        // GET: api/tabs
        [HttpGet]
        public async Task<IActionResult> GetTabs([FromQuery] string? status = null)
        {
            var data = await _tabsService.GetTabsAsync(status);
            return Ok(data);
        }

        // GET: api/tabs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTab(int id)
        {
            var tab = await _tabsService.GetTabAsync(id);
            if (tab == null) return NotFound();
            return Ok(tab);
        }

        // POST: api/tabs
        [HttpPost]
        public async Task<IActionResult> OpenTab(CreateTabDto dto)
        {
            var result = await _tabsService.OpenTabAsync(dto);
            // Quick hack for CreatedAtAction - just using reflection or returning Ok
            return Ok(result);
        }

        // POST: api/tabs/{id}/items
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, AddTabItemDto dto)
        {
            try
            {
                var message = await _tabsService.AddItemAsync(id, dto);
                return Ok(new { message });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("não encontrad")) return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/tabs/{id}/items/{itemId}
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int id, int itemId)
        {
            try
            {
                var message = await _tabsService.RemoveItemAsync(id, itemId);
                return Ok(new { message });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("não encontrad")) return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // POST: api/tabs/{id}/close
        [HttpPost("{id}/close")]
        public async Task<IActionResult> CloseTab(int id, CloseTabDto dto)
        {
            try
            {
                var result = await _tabsService.CloseTabAsync(id, dto);
                var dict = (dynamic)result;
                return Ok(new { message = "Comanda fechada com sucesso.", saleId = dict.saleId, total = dict.total });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("não encontrad")) return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // POST: api/tabs/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelTab(int id, [FromBody] CancelActionDto dto)
        {
            try
            {
                var message = await _tabsService.CancelTabAsync(id, dto);
                return Ok(new { message });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("não encontrad")) return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
