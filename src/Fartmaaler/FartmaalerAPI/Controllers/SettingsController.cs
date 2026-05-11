using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/settings — hent alle indstillinger (alle kan se dem)
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var settings = _context.Settings.ToList();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Der opstod en fejl", error = ex.Message });
            }
        }

        // PUT /api/settings/{key} — opdater en indstilling (kun admin)
        [Authorize(Roles = "admin")]
        [HttpPut("{key}")]
        public IActionResult Update(string key, [FromBody] bool value)
        {
            try
            {
                var setting = _context.Settings.FirstOrDefault(s => s.Key.ToLower() == key.ToLower());

                if (setting == null)
                    return NotFound(new { message = "Indstillingen blev ikke fundet" });

                setting.Value = value;
                _context.SaveChanges();

                return Ok(setting);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Der opstod en fejl", error = ex.Message });
            }
        }
    }
}