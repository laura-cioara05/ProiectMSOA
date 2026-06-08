using System;
using System.Collections.Generic;
using System.Text;

namespace GymFitnessSystem.Models.Produs_Serviciu
{
    public class Serviciu:OfertaBase
    {
        public int DurataZile { get; set; } 
        public bool NecesitaProgramare { get; set; } // True pentru antrenor, False pentru abonament simplu
    }
}
