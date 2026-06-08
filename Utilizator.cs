using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GymFitnessSystem.Models
{
    public class Utilizator : IDataErrorInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Parola { get; set; } = string.Empty;

        [Required]
        public RolUtilizator Rol { get; set; } = RolUtilizator.Client;
        [Required]
        [MaxLength(100)]
        public string NumeComplet { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telefon { get; set; } = string.Empty;

        // Atribute pentru Administrare și Status
        public DateTime DataCreare { get; set; } = DateTime.Now;

        public bool EsteActiv { get; set; } = true; // Pentru a putea bloca un cont fără a-l șterge
        public bool? EsteAntrenor { get; set; } = false; // Implicit e false pentru clienți

        // --- LOGICA DE VALIDARE IDataErrorInfo ---

        public string Error => null;

        // Această proprietate verifică automat fiecare câmp când se modifică în tabel sau pop-up
        
        public Utilizator()
        {
            DataCreare = DateTime.Now; // Sau DateTime.Today dacă vrei doar data fără oră/secunde
            EsteActiv = true;          // Îl facem activ direct la creare
        }

        //  INDEXATORUL IDataErrorInfo 
        public string this[string columnName]
        {
            get
            {
                string result = null;

                switch (columnName)
                {
                    case nameof(Username):
                        if (string.IsNullOrWhiteSpace(Username))
                            result = "Username-ul este obligatoriu!";
                        break;

                    // NOU: Validare pentru NumeComplet (Câmpul devine obligatoriu)
                    case nameof(NumeComplet):
                        if (string.IsNullOrWhiteSpace(NumeComplet))
                            result = "Numele complet este obligatoriu!";
                        break;

                    case nameof(Parola):
                        // Corectat: Condiția și textul cer acum strict minimum 3 caractere
                        if (string.IsNullOrWhiteSpace(Parola) || Parola.Length < 3)
                            result = "Parola trebuie să aibă minimum 3 caractere!";
                        break;

                    case nameof(Email):
                        if (string.IsNullOrWhiteSpace(Email))
                        {
                            result = "Email-ul este obligatoriu!";
                        }
                        // Adaptat după regulile tale de domenii specifice
                        else if (!Email.Contains("@") || (!Email.EndsWith("@gmail.com") &&
                                                         !Email.EndsWith("@yahoo.com") &&
                                                         !Email.EndsWith("@outlook.com")))
                        {
                            result = "Email invalid! Trebuie să se termine în @gmail.com, @yahoo.com sau @outlook.com.";
                        }
                        break;

                    case nameof(Telefon):
                        // Modificat: Telefonul este acum OBLIGATORIU (nu mai lăsăm spații goale)
                        if (string.IsNullOrWhiteSpace(Telefon))
                        {
                            result = "Numărul de telefon este obligatoriu!";
                        }
                        else
                        {
                            string telCurat = Telefon.Trim();
                            if (telCurat.Length != 10)
                            {
                                result = "Numărul de telefon trebuie să aibă exact 10 cifre!";
                            }
                            else
                            {
                                foreach (char c in telCurat)
                                {
                                    if (!char.IsDigit(c))
                                    {
                                        result = "Telefonul poate conține doar cifre!";
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                }

                return result;
            }
        }
    }

    

    public enum RolUtilizator
    {
        Administrator,
        Angajat,
        Client
    }
}