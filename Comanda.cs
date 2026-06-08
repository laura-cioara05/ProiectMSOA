using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymFitnessSystem.Models.Comanda_Programare
{
    public class Comanda : ActivitateBase
    {
        [Key, ForeignKey("Activitate")]
        public new int Id { get; set; }

        public virtual ActivitateBase Activitate { get; set; }
        public int Cantitate { get; set; }

        public StatusComanda StatusLivrare { get; set; } = StatusComanda.InProcesare;
    }

    // Noul Enum pentru controlul comenzilor de la Bar
    public enum StatusComanda
    {
        InProcesare,
        LivratLaReceptie,
        Ridicata,
        Anulata
    }
}
