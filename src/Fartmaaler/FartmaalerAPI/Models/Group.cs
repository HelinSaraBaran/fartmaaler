namespace FartmaalerAPI.Models;

// Denne klasse repræsenterer en gruppe i systemet
// En gruppe kan fx være en skoleklasse, der bruger fartmåleren
public class Group
{
    // Unik identifikator for gruppen (primary key i databasen)
    public int Id { get; set; }

    // Navnet på gruppen (fx "8.A")
    public string Name { get; set; }

    // Navnet på skolen gruppen tilhører
    public string School { get; set; }

    // Angiver om gruppen er låst
    // Hvis true kan gruppen ikke ændres eller bruges aktivt
    public bool IsLocked { get; set; }
}