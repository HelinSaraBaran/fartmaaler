using Microsoft.AspNetCore.Mvc;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FunFactsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FunFactsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("random")]
        public IActionResult GetRandomFunFact()
        {
            List<FunFact> funFacts =
                _context.FunFacts.ToList();

            if (funFacts.Count == 0)
            {
                return Ok(new
                {
                    text = "Vidste du at jævn fart ofte bruger mindre energi?"
                });
            }

            Random random =
                new Random();

            int index =
                random.Next(funFacts.Count);

            FunFact selectedFunFact =
                funFacts[index];

            return Ok(new
            {
                id = selectedFunFact.Id,
                text = selectedFunFact.Text
            });
        }

        [HttpGet]
        public IActionResult GetAllFunFacts()
        {
            List<FunFact> funFacts =
                _context.FunFacts.ToList();

            return Ok(funFacts);
        }
    }
}