using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace GymFitnessSystem.Views.Client
{
    public partial class UcComenzi : UserControl
    {
        public UcComenzi()
        {
            InitializeComponent();
            IncarcaActivitatilCurente();
        }

        private void IncarcaActivitatilCurente()
        {
            int idClient = UserSession.LoggedInUserId;

            try
            {
                using (var context = new GymContext())
                {
                    
                    //  POPULARE REZERVĂRI / PROGRAMĂRI ACTIVE (ÎN CURS)
                    
                    var programariDinDb = context.Programari
                        .Include(p => p.Activitate)
                        .Include(p => p.Antrenor)
                        // FILTRARE: Luăm doar programările clientului logat care sunt încă active (InAsteptare)
                        // Astfel, cele cu status "Prezent" sau "Anulat" DISPAR instant de pe ecran
                        .Where(p => p.UtilizatorId == idClient && p.Status == StatusPrezenta.InAsteptare)
                        .ToList();

                    var listaProgramariAfisare = programariDinDb.Select(p =>
                    {
                        Brush culoare = Brushes.Orange; // Portocaliu pentru active/în așteptare
                        string statusText = "În Așteptare";

                        // Căutăm numele serviciului din Oferta/Serviciu folosind OfertaId din ActivitateBase
                        string numeServiciu = context.Servicii.FirstOrDefault(s => s.Id == p.Activitate.OfertaId)?.Nume ?? "Ședință Fitness";

                        // EXTRAGERE METODĂ DE PLATĂ PENTRU PROGRAMARE
                        var plata = context.Plati.FirstOrDefault(pl => pl.ActivitateId == p.Activitate.Id);
                        string tipPlata = "Nespecificată";

                        if (plata != null)
                        {
                            if (plata.Metoda.ToString() == "0" || plata.Metoda.ToString().Contains("Cash", StringComparison.OrdinalIgnoreCase))
                            {
                                tipPlata = "Cash";
                            }
                            else if (plata.Metoda.ToString() == "1" || plata.Metoda.ToString().Contains("Card", StringComparison.OrdinalIgnoreCase))
                            {
                                tipPlata = "Card";
                            }
                            else
                            {
                                tipPlata = plata.Metoda.ToString();
                            }
                        }

                        return new ProgramareUIItem
                        {
                            NumeServiciu = numeServiciu,
                            DataOraText = $"Data: {p.DataOraProgramata:dd.MM.yyyy} | Oră: {p.DataOraProgramata:HH:mm}",
                            AntrenorText = p.AntrenorId != null ? $"Antrenor: {p.Antrenor.NumeComplet}" : "Fără antrenor personal",
                            StatusText = statusText,
                            MetodaPlata = $"💳 Plătit prin: {tipPlata}", // <--- Îi pasăm proprietatea adăugată
                            CuloareStare = culoare
                        };
                    }).ToList();

                    icProgramariServicii.ItemsSource = listaProgramariAfisare;


                    
                    // POPULARE COMENZI PRODUSE (BAR) - DOAR CELE ÎN CURS
                    
                    var comenziDinDb = context.Comenzi
                        .Include(c => c.Activitate)
                        .Where(c => c.Activitate.UtilizatorId == idClient
                                 && c.StatusLivrare != StatusComanda.Ridicata
                                 && c.StatusLivrare != StatusComanda.Anulata)
                        .ToList();

                    var listaComenziAfisare = comenziDinDb.Select(c =>
                    {
                        Brush culoare = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")); // Portocaliu implicit
                        string statusText = "În Procesare";

                        if (c.StatusLivrare == StatusComanda.LivratLaReceptie)
                        {
                            culoare = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // Verde
                            statusText = "Disponibil la Recepție";
                        }

                        var oferta = context.Oferte.FirstOrDefault(o => o.Id == c.Activitate.OfertaId);
                        string numeProdus = oferta != null ? oferta.Nume : "Produs Bar";
                        decimal pretUnitar = oferta != null ? (decimal)oferta.Pret : 0;
                        decimal pretTotalCalculat = pretUnitar * c.Cantitate;

                        var plata = context.Plati.FirstOrDefault(p => p.ActivitateId == c.Activitate.Id);
                        string tipPlata = "Nespecificată";

                        if (plata != null)
                        {
                            if (plata.Metoda.ToString() == "0" || plata.Metoda.ToString().Contains("Cash", StringComparison.OrdinalIgnoreCase))
                            {
                                tipPlata = "Cash";
                            }
                            else if (plata.Metoda.ToString() == "1" || plata.Metoda.ToString().Contains("Card", StringComparison.OrdinalIgnoreCase))
                            {
                                tipPlata = "Card";
                            }
                            else
                            {
                                tipPlata = plata.Metoda.ToString();
                            }
                        }

                        return new ComandaUIItem
                        {
                            NumeProdus = numeProdus,
                            DetaliiCantitate = $"Cantitate: {c.Cantitate} buc. | Total: {pretTotalCalculat:F2} RON",
                            StatusText = statusText,
                            MetodaPlata = $"💳 Plătit prin: {tipPlata}",
                            CuloareStare = culoare
                        };
                    }).ToList();

                    icComenziProduse.ItemsSource = listaComenziAfisare;

                    
                    //  POPULARE ABONAMENTE PURE DIN ACTIVITATEBASE
                    
                    // Extragem ID-urile derivatelor pentru a asigura compatibilitate maximă cu orice tip de mapare EF (TPT/TPH)
                    var idUriComenzi = context.Comenzi.Select(c => c.Id).ToList();
                    var idUriProgramari = context.Programari.Select(p => p.Id).ToList();

                    // Interogăm direct tabela părinte ActivitateBase (context.Activitati)
                    var activitatiBrute = context.Activitati
                        .Include(a => a.Oferta)
                        .Where(a => a.UtilizatorId == idClient
                                 && !idUriComenzi.Contains(a.Id)
                                 && !idUriProgramari.Contains(a.Id))
                        .ToList();

                    var listaAbonamenteAfisare = activitatiBrute.Select(a =>
                    {
                        // Căutăm în tabela Plati înregistrarea legată de această activitate de bază
                        var plata = context.Plati.FirstOrDefault(p => p.ActivitateId == a.Id);

                        // Dacă nu are plată sau plata nu e în așteptare, înseamnă că nu e o acțiune curentă cash pending
                        if (plata == null || plata.Status != StatusPlata.InAsteptare)
                            return null;

                        Brush culoare = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0EA5E9")); // Sky Blue

                        return new AbonamentUIItem
                        {
                            NumeAbonament = a.Oferta != null ? a.Oferta.Nume : "Abonament / Intrare Sală",
                            DetaliiPret = $"Preț Achiziție: {plata.Suma:F2} RON",
                            MetodaPlata = $"💳 Plătit prin: {plata.Metoda}",
                            StatusText = "Așteaptă Încasare",
                            CuloareStare = culoare
                        };
                    })
                    .Where(item => item != null) // Curățăm elementele care au fost excluse la validarea plății
                    .ToList();

                    icAbonamenteCurente.ItemsSource = listaAbonamenteAfisare;

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Eroare la încărcarea statusului: {ex.Message}");
            }
        }
    }

    // Clase Helper (DTOs) pentru a lega datele curat în XAML fără bătăi de cap
    public class ProgramareUIItem
    {
        public string NumeServiciu { get; set; }
        public string DataOraText { get; set; }
        public string AntrenorText { get; set; }
        public string StatusText { get; set; }

        public string MetodaPlata { get; set; }
        public Brush CuloareStare { get; set; }
    }

    public class ComandaUIItem
    {
        public string NumeProdus { get; set; }
        public string DetaliiCantitate { get; set; }
        public string StatusText { get; set; }

        public string MetodaPlata { get; set; }
        public Brush CuloareStare { get; set; }
    }

    public class AbonamentUIItem
    {
        public string NumeAbonament { get; set; }
        public string DetaliiPret { get; set; }
        public string MetodaPlata { get; set; }
        public string StatusText { get; set; }
        public Brush CuloareStare { get; set; }
    }
}