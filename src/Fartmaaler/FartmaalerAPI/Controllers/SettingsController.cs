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

        // Henter alle indstillinger.
        // Elever og frontend må gerne læse indstillingerne.
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                List<Settings> settings =
                    _context.Settings.ToList();

                return Ok(settings);
            }
            catch (Exception exception)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "Der opstod en fejl",
                        error = exception.Message
                    }
                );
            }
        }

        // Opdaterer en global indstilling.
        // Både admin og teacher må ændre indstillinger.
        [Authorize(Roles = "admin,teacher,Teacher")]
        [HttpPut("{key}")]
        public IActionResult Update(string key, [FromBody] Settings updatedSetting)
        {
            try
            {
                Settings? setting =
                    _context.Settings.FirstOrDefault(
                        settingItem => settingItem.Key.ToLower() == key.ToLower()
                    );

                if (setting == null)
                {
                    return NotFound(
                        new
                        {
                            message = "Indstillingen blev ikke fundet"
                        }
                    );
                }

                setting.Value =
                    updatedSetting.Value;

                _context.SaveChanges();

                return Ok(setting);
            }
            catch (Exception exception)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "Der opstod en fejl",
                        error = exception.Message
                    }
                );
            }
        }
    }
}