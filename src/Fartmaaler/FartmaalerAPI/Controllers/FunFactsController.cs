using Microsoft.AspNetCore.Mvc;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FunFactsController : ControllerBase
    {
        // Henter en tilfældig fun fact
        [HttpGet("random")]
        public IActionResult GetRandomFunFact()
        {
            List<string> funFacts = new List<string>
            {
                "Hvis alle sænkede hastigheden lidt, kunne man reducere brændstofforbrug og CO2 udledning.",
                "En cykel udleder 0 gram CO2 under selve kørslen.",
                "Jævn hastighed bruger ofte mindre energi end mange hårde accelerationer.",
                "Lavere hastighed kan give bedre kontrol og mindre energiforbrug.",
                "Når hastigheden stiger, kræver det mere energi at overvinde luftmodstand."
            };

            Random random = new Random();
            int index = random.Next(funFacts.Count);

            return Ok(new
            {
                text = funFacts[index]
            });
        }

        // Henter alle fun facts
        [HttpGet]
        public IActionResult GetAllFunFacts()
        {
            List<string> funFacts = new List<string>
            {
                "Hvis alle sænkede hastigheden lidt, kunne man reducere brændstofforbrug og CO2 udledning.",
                "En cykel udleder 0 gram CO2 under selve kørslen.",
                "Jævn hastighed bruger ofte mindre energi end mange hårde accelerationer.",
                "Lavere hastighed kan give bedre kontrol og mindre energiforbrug.",
                "Når hastigheden stiger, kræver det mere energi at overvinde luftmodstand."
            };

            return Ok(funFacts);
        }
    }
}