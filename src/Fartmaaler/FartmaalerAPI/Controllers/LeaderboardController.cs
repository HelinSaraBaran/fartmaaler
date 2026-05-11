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

        // Constructor modtager leaderboard service
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
                return BadRequest(new { message = "Vejtype skal udfyldes" }); // 400

            List<LeaderboardEntryResponse> result = _leaderboardService.GetClassLeaderboard(roadType);

            if (!result.Any())
                return NotFound(new { message = "Ingen afsluttede sessions for denne vejtype" }); // 404

            return Ok(result); // 200
        }

        // Admin henter skole leaderboard
        [Authorize(Roles = "admin")]
        [HttpGet("admin/school")]
        public IActionResult GetAdminSchoolLeaderboard(string roadType)
        {
            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" }); // 400

            List<SchoolLeaderboardResponse> result = _leaderboardService.GetSchoolLeaderboard(roadType);

            if (!result.Any())
                return NotFound(new { message = "Ingen skoledata for denne vejtype" }); // 404

            return Ok(result); // 200
        }

        // Elev henter klasse leaderboard
        [HttpGet("student/class")]
        public IActionResult GetStudentClassLeaderboard(string roadType)
        {
            if (!_leaderboardService.IsLeaderboardEnabled())
                return Forbid(); // 403

            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" }); // 400

            List<LeaderboardEntryResponse> result = _leaderboardService.GetClassLeaderboard(roadType);

            if (!result.Any())
                return NotFound(new { message = "Ingen afsluttede sessions for denne vejtype" }); // 404

            return Ok(result); // 200
        }

        // Elev henter skole leaderboard
        [HttpGet("student/school")]
        public IActionResult GetStudentSchoolLeaderboard(string roadType)
        {
            if (!_leaderboardService.IsLeaderboardEnabled())
                return Forbid(); // 403

            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" }); // 400

            List<SchoolLeaderboardResponse> result = _leaderboardService.GetSchoolLeaderboard(roadType);

            if (!result.Any())
                return NotFound(new { message = "Ingen skoledata for denne vejtype" }); // 404

            return Ok(result); // 200
        }

        // Admin ser om leaderboard er slået til
        [Authorize(Roles = "admin")]
        [HttpGet("setting")]
        public IActionResult GetLeaderboardSetting()
        {
            bool isEnabled = _leaderboardService.IsLeaderboardEnabled();

            return Ok(new { isEnabled = isEnabled }); // 200
        }

        // Admin slår leaderboard til eller fra
        [Authorize(Roles = "admin")]
        [HttpPut("setting")]
        public IActionResult UpdateLeaderboardSetting([FromBody] UpdateLeaderboardSettingRequest request)
        {
            LeaderboardSetting setting = _leaderboardService.UpdateLeaderboardSetting(request.IsEnabled);

            return Ok(setting); // 200
        }
    }
}