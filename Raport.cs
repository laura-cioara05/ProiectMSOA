using GymFitnessSystem.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymFitnessSystem.Models
{
    public class Raport
    {
        public int Id { get; set; }

        // Tipul raportului: "Vânzări", "Prezență", "Stocuri"
        public TipRaport Tip { get; set; } = TipRaport.Vanzari;
        public DateTime DataGenerare { get; set; } = DateTime.Now;

        // Cine a generat raportul
        public int GeneratDeUtilizatorId { get; set; }
        public virtual Utilizator? GeneratDe { get; set; }

        // Conținutul raportului
        public string DetaliiSauPath { get; set; } = string.Empty;

        //  Adaugă o perioadă pentru care a fost generat (ex: "Ianuarie 2026")
        public string PerioadaVizat { get; set; } = string.Empty;
    }
    public enum TipRaport
    {
        Vanzari,
        Prezenta,
        Stocuri
    }
}
