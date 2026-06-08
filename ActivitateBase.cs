using GymFitnessSystem.Models.Produs_Serviciu;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymFitnessSystem.Models.Comanda_Programare
{
    public class ActivitateBase
    {
        public int Id { get; set; }

        // Cine?
        public  int UtilizatorId { get; set; }
        public virtual Utilizator Utilizator { get; set; }

        // Ce? (Aici facem legătura cu OfertaBase creată anterior)
        public int OfertaId { get; set; }
        public virtual OfertaBase Oferta { get; set; }

        public decimal PretAchitat { get; set; } // Salvăm prețul din momentul tranzacției
        public DateTime DataInregistrare { get; set; } = DateTime.Now;
    }
}
