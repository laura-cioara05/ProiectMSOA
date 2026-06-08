using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymFitnessSystem.Models.Comanda_Programare
{
    public class Programare: ActivitateBase
    {
        [Key, ForeignKey("Activitate")]
        public new int Id { get; set; }

        public virtual ActivitateBase Activitate { get; set; }
        public DateTime DataOraProgramata { get; set; }
        public StatusPrezenta Status { get; set; } = StatusPrezenta.InAsteptare;
        // Opțional: Legătură către antrenor (care este tot un Utilizator cu rol de Angajat)
        public int? AntrenorId { get; set; }
        public virtual Utilizator? Antrenor { get; set; }
    }

    // Enum-ul pentru starea unei programări
    public enum StatusPrezenta
    {
        InAsteptare,
        Prezent,
        Anulat
    }
}
