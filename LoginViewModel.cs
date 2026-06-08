using GymFitnessSystem.Data;
using System;
using System.Collections.Generic;
using GymFitnessSystem.Models;
using System.Text;

namespace GymFitnessSystem.ViewModels
{
    public class LoginViewModel
    {
        // Metoda care verifica daca user-ul si parola exista in baza de date

        // AUTENTIFICARE (Login)
        public Utilizator? VerificaLogin(string username, string parola)
        {
            using (var db = new GymContext())
            {
                // Cautam utilizatorul care are username-ul si parola introduse
                return db.Utilizatori.FirstOrDefault(u => u.Username == username && u.Parola == parola);
            }
        }

        // ÎNREGISTRARE (Register)
        public bool InregistrareUtilizator(string username, string parola, string email, string nume, string telefon)
        {
            using (var db = new GymContext())
            {
                //  VERIFICARE DUPLICATE (Username SAU Email)
                // Dacă un Admin sau altcineva are deja acest email, returnăm false.
                if (db.Utilizatori.Any(u => u.Username == username || u.Email == email))
                {
                    return false;
                }

                //  CREARE OBIECT NOU
                var nouUtilizator = new Utilizator
                {
                    Username = username,
                    Parola = parola, // Într-un sistem real am folosi hashing, dar rămânem pe simplu acum
                    Email = email,
                    NumeComplet = nume,
                    Telefon = telefon,
                    Rol = RolUtilizator.Client,      // Toți cei care se înregistrează primesc rol de Client
                    DataCreare = DateTime.Now,
                    EsteActiv = true     // Contul este activ implicit
                };

                //  SALVARE ÎN BAZA DE DATE
                db.Utilizatori.Add(nouUtilizator);
                db.SaveChanges();
                return true;
            }
        }
    }
}
