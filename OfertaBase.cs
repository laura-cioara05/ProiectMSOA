using System;
using System.Collections.Generic;
using System.Text;

namespace GymFitnessSystem.Models.Produs_Serviciu
{
    public abstract class OfertaBase//Produs/Serviciu
    {
        public int Id { get; set; }
        public string Nume { get; set; } = string.Empty;
        public string Descriere { get; set; } = string.Empty;
        public decimal Pret { get; set; }

    }
}
