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

        [Authorize(Roles = "admin")]
        [HttpGet("admin/class")]
        public IActionResult GetAdminClassLeaderboard(string roadType)
        {
            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" });

            List<LeaderboardEntryResponse> result =
                _leaderboardService.GetClassLeaderboard(roadType);

            if (!result.Any())
            {
                return Ok(new
                {
                    message = "Ingen afsluttede sessions for denne vejtype",
                    leaderboard = result
                });
            }

            return Ok(new
            {
                leaderboard = result
            });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("admin/school")]
        public IActionResult GetAdminSchoolLeaderboard(string roadType)
        {
            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" });

            List<SchoolLeaderboardResponse> result =
                _leaderboardService.GetSchoolLeaderboard(roadType);

            if (!result.Any())
            {
                return Ok(new
                {
                    message = "Ingen skoledata for denne vejtype",
                    leaderboard = result
                });
            }

            return Ok(new
            {
                leaderboard = result
            });
        }

        [HttpGet("student/class")]
        public IActionResult GetStudentClassLeaderboard(string roadType)
        {
            if (!_leaderboardService.IsLeaderboardEnabled())
                return Forbid();

            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" });

            List<LeaderboardEntryResponse> result =
                _leaderboardService.GetClassLeaderboard(roadType);

            if (!result.Any())
            {
                return Ok(new
                {
                    message = "Ingen afsluttede sessions for denne vejtype",
                    leaderboard = result
                });
            }

            return Ok(new
            {
                leaderboard = result
            });
        }

        [HttpGet("student/school")]
        public IActionResult GetStudentSchoolLeaderboard(string roadType)
        {
            if (!_leaderboardService.IsLeaderboardEnabled())
                return Forbid();

            if (string.IsNullOrWhiteSpace(roadType))
                return BadRequest(new { message = "Vejtype skal udfyldes" });

            List<SchoolLeaderboardResponse> result =
                _leaderboardService.GetSchoolLeaderboard(roadType);

            if (!result.Any())
            {
                return Ok(new
                {
                    message = "Ingen skoledata for denne vejtype",
                    leaderboard = result
                });
            }

            return Ok(new
            {
                leaderboard = result
            });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("setting")]
        public IActionResult GetLeaderboardSetting()
        {
            bool isEnabled = _leaderboardService.IsLeaderboardEnabled();

            return Ok(new { isEnabled = isEnabled });
        }

        [Authorize(Roles = "admin")]
        [HttpPut("setting")]
        public IActionResult UpdateLeaderboardSetting([FromBody] UpdateLeaderboardSettingRequest request)
        {
            Settings setting = _leaderboardService.UpdateLeaderboardSetting(request.IsEnabled);

            return Ok(setting);
        }
    }
}