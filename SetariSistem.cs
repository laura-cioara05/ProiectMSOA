using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;


namespace GymFitnessSystem.Models
{
    public class SetariSistem
    {
        [Key]
        public int Id { get; set; }
        public string Adresa { get; set; }
        public string Program { get; set; }
        public string TelefonSuport { get; set; }
    }
}