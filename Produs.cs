using System;
using System.Collections.Generic;
using System.Text;

namespace GymFitnessSystem.Models.Produs_Serviciu
{
    public class Produs:OfertaBase
    {
        public int Stoc { get; set; }
        public string Producator { get; set; } = string.Empty;
    }
}
