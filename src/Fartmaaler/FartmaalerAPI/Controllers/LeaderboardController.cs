using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        // Admin henter klasse leaderboard
        [Authorize(Roles = "admin")]
        [HttpGet("admin/class")]
        public IActionResult GetAdminClassLeaderboard(string roadType)
        {
            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" });

            List<LeaderboardEntryResponse> result = _leaderboardService.GetClassLeaderboard(roadType);

            if (!result.Any())
                return NotFound(new { message = "Ingen afsluttede sessions for denne vejtype" });

            return Ok(result);
        }

        // Admin henter skole leaderboard
        [Authorize(Roles = "admin")]
        [HttpGet("admin/school")]
        public IActionResult GetAdminSchoolLeaderboard(string roadType)
        {
            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" });

            List<SchoolLeaderboardResponse> result = _leaderboardService.GetSchoolLeaderboard(roadType);

            if (!result.Any())
                return NotFound(new { message = "Ingen skoledata for denne vejtype" });

            return Ok(result);
        }

        // Elev henter klasse leaderboard hvis det er slået til
        [HttpGet("student/class")]
        public IActionResult GetStudentClassLeaderboard(string roadType)
        {
            if (!_leaderboardService.IsLeaderboardEnabled())
                return Forbid();

            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" });

            List<LeaderboardEntryResponse> result = _leaderboardService.GetClassLeaderboard(roadType);

            if (!result.Any())
                return NotFound(new { message = "Ingen afsluttede sessions for denne vejtype" });

            return Ok(result);
        }

        // Elev henter skole leaderboard hvis det er slået til
        [HttpGet("student/school")]
        public IActionResult GetStudentSchoolLeaderboard(string roadType)
        {
            if (!_leaderboardService.IsLeaderboardEnabled())
                return Forbid();

            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" });

            List<SchoolLeaderboardResponse> result = _leaderboardService.GetSchoolLeaderboard(roadType);

            if (!result.Any())
                return NotFound(new { message = "Ingen skoledata for denne vejtype" });

            return Ok(result);
        }

        // Admin ser om leaderboard er slået til
        [Authorize(Roles = "admin")]
        [HttpGet("setting")]
        public IActionResult GetLeaderboardSetting()
        {
            bool isEnabled = _leaderboardService.IsLeaderboardEnabled();

            return Ok(new { isEnabled = isEnabled });
        }

        // Admin slår leaderboard til eller fra
        [Authorize(Roles = "admin")]
        [HttpPut("setting")]
        public IActionResult UpdateLeaderboardSetting([FromBody] UpdateLeaderboardSettingRequest request)
        {
            Settings setting = _leaderboardService.UpdateLeaderboardSetting(request.IsEnabled);

            return Ok(setting);
        }
    }
}