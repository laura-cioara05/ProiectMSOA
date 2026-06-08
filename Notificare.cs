using GymFitnessSystem.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymFitnessSystem.Models
{
    public class Notificare
    {
        public int Id { get; set; }

        // Legătura cu utilizatorul care primește notificarea
        public int UtilizatorId { get; set; }
        public virtual Utilizator? Utilizator { get; set; }

        public string Mesaj { get; set; } = string.Empty;
        public DateTime DataTrimitere { get; set; } = DateTime.Now;
        public bool EsteCitit { get; set; } = false;

        // SFAT: Adaugă și un Titlu, e util pentru UI (ex: "Stoc epuizat" sau "Programare nouă")
        public string Titlu { get; set; } = "Notificare Sistem";
    }
}
