using GymFitnessSystem.Models.Comanda_Programare;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace GymFitnessSystem.Models
{
    public class Plata
    {
        public int Id { get; set; }

        // Acum se leagă de ActivitateBase (care poate fi ori Comandă, ori Programare)
        public int ActivitateId { get; set; }
        public virtual ActivitateBase? Activitate { get; set; }

        public decimal Suma { get; set; }
        public DateTime DataPlatii { get; set; } = DateTime.Now;
        public MetodaPlata Metoda { get; set; } = MetodaPlata.Cash;
        public StatusPlata Status { get; set; } = StatusPlata.Neplatit;
   
        [NotMapped] // Nu o salvăm în DB, doar pentru afișare
        public string NumeArticol { get; set; }
    }

    public enum MetodaPlata
    {
        Cash,
        Card
    }

    public enum StatusPlata
    {
        Neplatit,
        InAsteptare, 
        Platit,
        Anulat
    }
}
