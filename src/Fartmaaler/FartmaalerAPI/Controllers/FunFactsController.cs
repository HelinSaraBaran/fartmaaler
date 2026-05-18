using Microsoft.AspNetCore.Mvc;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FunFactsController : ControllerBase
    {
        // Henter en tilfældig fun fact.
        // Tallene er pædagogiske estimater og ikke præcise beregninger for den enkelte bil.
        [HttpGet("random")]
        public IActionResult GetRandomFunFact()
        {
            List<string> funFacts = GetFunFacts();

            Random random = new Random();
            int index = random.Next(funFacts.Count);

            return Ok(new
            {
                text = funFacts[index]
            });
        }

        // Henter alle fun facts.
        [HttpGet]
        public IActionResult GetAllFunFacts()
        {
            return Ok(GetFunFacts());
        }

        // Samler alle fun facts ét sted.
        private List<string> GetFunFacts()
        {
            List<string> funFacts = new List<string>
            {
                "Cykling og gang udleder 0 gram CO₂ under selve turen. Hvis du cykler i stedet for at tage bilen på korte ture, sparer du derfor bilens direkte udledning.",

                "Transport står for cirka en fjerdedel af EU’s samlede drivhusgasudledning. Derfor betyder vores transportvaner meget for klimaet.",

                "Hvis du vælger cykel eller gang bare én dag om ugen i stedet for bilen, kan det være med til at reducere din transportudledning over tid.",

                "Jævn kørsel kan bruge mindre energi end hårde accelerationer og pludselige opbremsninger. Derfor handler bæredygtig kørsel ikke kun om hastighed, men også om kørestil.",

                "Jo hurtigere en bil kører, jo mere energi skal den bruge på at overvinde luftmodstand. Derfor kan meget høj fart øge energiforbruget.",

                "Hvis en klasse ofte vælger cykel, gang eller offentlig transport til korte ture, kan den samlede CO₂-besparelse blive stor over et skoleår.",

                "Hvis du sparer penge ved at tage bilen færre gange, kan de penge over tid bruges på noget andet. En dyr telefon som en iPhone Pro Max kan koste omkring 10.000-14.000 kr. afhængigt af model og lagerplads.",

                "Hvis en biltur udleder omkring 150 gram CO₂ pr. kilometer som et simpelt estimat, vil en cykeltur på 5 km spare omkring 750 gram CO₂ sammenlignet med bilen.",

                "Hvis du undgår 10 korte bilture på 5 km, og bilen udleder cirka 150 gram CO₂ pr. kilometer, svarer det til omkring 7,5 kg CO₂ sparet.",

                "Hvis du cykler 5 km i stedet for at køre bil, kan du både spare CO₂, spare brændstofpenge og få motion på samme tid.",

                "Små valg tæller: én kort biltur virker lille, men mange korte ture hver uge kan blive til meget CO₂ over et helt år.",

                "Bæredygtig transport handler ikke kun om at køre langsomt. Det handler også om at vælge det rigtige transportmiddel til turen."
            };

            return funFacts;
        }
    }
}