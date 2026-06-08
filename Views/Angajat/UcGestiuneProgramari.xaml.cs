using GymFitnessSystem.Data;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymFitnessSystem.Services;

namespace GymFitnessSystem.Views.Angajat
{
    public partial class UcGestiuneProgramari : UserControl
    {
        public UcGestiuneProgramari()
        {
            InitializeComponent();
            IncarcaDateMixte();
        }

        private void IncarcaDateMixte()
        {
            try
            {
                using (var context = new GymContext())
                {
                    string textCautat = txtCautareProgramari.Text.Trim().ToLower();
                    int selectieFiltru = cbFiltruTip.SelectedIndex; // 0 = Toate, 1 = Programari, 2 = Abonamente

                    List<GestiuneElementUI> listaUnificata = new List<GestiuneElementUI>();

                    
                    // ÎNCĂRCARE PROGRAMĂRI (Clase / Antrenori)
                    
                    if (selectieFiltru == 0 || selectieFiltru == 1)
                    {
                        var programariDinDb = context.Programari
                            .Include(p => p.Utilizator)
                            .Include(p => p.Antrenor)
                            .Include(p => p.Activitate)
                                .ThenInclude(a => a.Oferta)
                            .ToList();

                        foreach (var p in programariDinDb)
                        {
                            
                            // Extragem întotdeauna ID-ul din tabela de bază ActivitateBase 
                            int idCorect = (p.Id == 0 && p.Activitate != null) ? p.Activitate.Id : p.Id;

                            // Aplicăm filtrul de căutare text
                            if (!string.IsNullOrEmpty(textCautat))
                            {
                                bool coincide = (p.Utilizator?.NumeComplet?.ToLower().Contains(textCautat) == true) ||
                                                (p.Utilizator?.Username?.ToLower().Contains(textCautat) == true);
                                if (!coincide) continue;
                            }

                            listaUnificata.Add(new GestiuneElementUI
                            {
                                IdReal = idCorect,
                                TipElement = "📅 Programare",
                                Username = p.Utilizator?.Username ?? "Nespecificat",
                                NumeComplet = p.Utilizator?.NumeComplet ?? "Nespecificat",
                                NumeOferta = p.Activitate?.Oferta?.Nume ?? "Ședință Curs",
                                DataOraText = p.DataOraProgramata.ToString("dd.MM.yyyy HH:mm"),
                                DataAsDateTime = p.DataOraProgramata,
                                AntrenorText = p.Antrenor != null ? p.Antrenor.NumeComplet : "Fără antrenor",
                                StatusText = p.Status.ToString(),
                                EsteAbonamentPur = false
                            });
                        }
                    }

                    
                    // ÎNCĂRCARE ABONAMENTE PURE (ActivitateBase)
                    
                    if (selectieFiltru == 0 || selectieFiltru == 2)
                    {
                        var idUriComenzi = context.Comenzi.Select(c => c.Id).ToList();
                        var idUriProgramari = context.Programari.Select(p => p.Id).ToList();

                        // Citim direct din tabela părinte elementele noderivate
                        var abonamenteBrute = context.Activitati
                            .Include(a => a.Utilizator)
                            .Include(a => a.Oferta)
                            .Where(a => !idUriComenzi.Contains(a.Id) && !idUriProgramari.Contains(a.Id))
                            .ToList();

                        foreach (var a in abonamenteBrute)
                        {
                            if (!string.IsNullOrEmpty(textCautat))
                            {
                                bool coincide = (a.Utilizator?.NumeComplet?.ToLower().Contains(textCautat) == true) ||
                                                (a.Utilizator?.Username?.ToLower().Contains(textCautat) == true);
                                if (!coincide) continue;
                            }

                            // Căutăm plata atașată pentru a-i vedea statusul
                            var plata = context.Plati.FirstOrDefault(p => p.ActivitateId == a.Id);
                            string statusFinanciar = plata != null ? plata.Status.ToString() : "FaraPlata";

                            listaUnificata.Add(new GestiuneElementUI
                            {
                                IdReal = a.Id, // ID-ul din ActivitateBase este garantat cel corect aici
                                TipElement = "💳 Abonament",
                                Username = a.Utilizator?.Username ?? "Nespecificat",
                                NumeComplet = a.Utilizator?.NumeComplet ?? "Nespecificat",
                                NumeOferta = a.Oferta?.Nume ?? "Abonament Sală",
                                DataOraText = "Valid 30 Zile (Permanent)",
                                DataAsDateTime = DateTime.MaxValue, // Împingem abonamentele jos/sus curat la sortare
                                AntrenorText = "Administrativ (Recepție)",
                                StatusText = statusFinanciar, // Va returna InAsteptare, Platit, Anulat
                                EsteAbonamentPur = true
                            });
                        }
                    }

                    // Ordonăm lista unificată astfel încât elementele în așteptare / active să fie prioritare
                    dgProgramari.ItemsSource = listaUnificata
                        .OrderBy(x => x.StatusText != "InAsteptare")
                        .ThenByDescending(x => x.DataAsDateTime)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la procesarea datelor mixte: {ex.Message}", "Eroare Sistem");
            }
        }

        private void TxtCautareProgramari_TextChanged(object sender, TextChangedEventArgs e)
        {
            IncarcaDateMixte();
        }

        private void CbFiltruTip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProgramari != null) // Evită prăbușirea la inițializarea componentei XAML
            {
                IncarcaDateMixte();
            }
        }

        private void BtnToate_Click(object sender, RoutedEventArgs e)
        {
            txtCautareProgramari.Clear();
            cbFiltruTip.SelectedIndex = 0;
            IncarcaDateMixte();
        }

        private void BtnPrezent_Click(object sender, RoutedEventArgs e)
        {
            var selectat = dgProgramari.SelectedItem as GestiuneElementUI;
            if (selectat == null)
            {
                MessageBox.Show("Te rog selectează un rând mai întâi!", "Atenție");
                return;
            }

            // Validare timp strict pentru programări. Pentru abonamente sărim peste ea!
            if (!selectat.EsteAbonamentPur)
            {
                if (DateTime.Now < selectat.DataAsDateTime.AddMinutes(-30))
                {
                    MessageBox.Show($"Check-in refuzat! Ședința este programată la ora {selectat.DataOraText}. Revino cu maxim 30 de minute înainte.", "Validare Timp");
                    return;
                }

                // Este programare clasică -> Apelăm modificarea de programări
                ModificaStatusElementMix(selectat.IdReal, true, false);
            }
            else
            {
                // Este abonament pur -> Executăm direct validarea plății
                ModificaStatusElementMix(selectat.IdReal, true, true);
            }
        }

        private void BtnAnuleazaProgramare_Click(object sender, RoutedEventArgs e)
        {
            var selectat = dgProgramari.SelectedItem as GestiuneElementUI;
            if (selectat == null) return;

            if (MessageBox.Show("Sigur dorești anularea acestei înregistrări?", "Confirmare", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ModificaStatusElementMix(selectat.IdReal, false, selectat.EsteAbonamentPur);
            }
        }

        private void ModificaStatusElementMix(int idReal, bool esteAprobare, bool esteAbonamentPur)
        {
            try
            {
                using (var context = new GymContext())
                {
                    if (!esteAbonamentPur)
                    {
                        //  Logica pentru PROGRAMĂRI (Clase / Antrenori)
                        var pDb = context.Programari
                            .Include(p => p.Activitate)
                                .ThenInclude(a => a.Oferta)
                            .FirstOrDefault(p => p.Id == idReal || p.Activitate.Id == idReal);

                        if (pDb != null)
                        {
                            pDb.Status = esteAprobare ? StatusPrezenta.Prezent : StatusPrezenta.Anulat;

                            // Actualizăm și plata legată de programare
                            var plataDb = context.Plati.FirstOrDefault(p => p.ActivitateId == idReal);
                            if (plataDb != null)
                            {
                                plataDb.Status = esteAprobare ? StatusPlata.Platit : StatusPlata.Anulat;
                                if (esteAprobare && plataDb.Metoda == MetodaPlata.Cash)
                                {
                                    plataDb.DataPlatii = DateTime.Now;
                                }
                            }

                            // CONEXIUNE INBOX PENTRU PROGRAMARE
                            int idClient = pDb.UtilizatorId;
                            string numeClasa = pDb.Activitate?.Oferta?.Nume ?? "Ședință Curs";
                            string statusText = pDb.Status.ToString();

                            ServiciuNotificari.TrimiteNotificareStatus(
                                idClient,
                                "PROGRAMARE",
                                statusText,
                                numeClasa
                            );
                        }
                    }
                    else
                    {
                        //  Logica pentru ABONAMENTE PURE
                        var aDb = context.Activitati
                            .Include(a => a.Oferta)
                            .FirstOrDefault(a => a.Id == idReal);

                        var plataAbonament = context.Plati.FirstOrDefault(p => p.ActivitateId == idReal);
                        if (plataAbonament != null && aDb != null)
                        {
                            plataAbonament.Status = esteAprobare ? StatusPlata.Platit : StatusPlata.Anulat;
                            if (esteAprobare)
                            {
                                plataAbonament.DataPlatii = DateTime.Now;
                            }

                            // CONEXIUNE INBOX PENTRU ABONAMENT
                            int idClient = aDb.UtilizatorId;
                            string numeAbonament = aDb.Oferta?.Nume ?? "Abonament Sală";
                            string statusPlataText = plataAbonament.Status.ToString();

                            ServiciuNotificari.TrimiteNotificareStatus(
                                idClient,
                                "ABONAMENT",
                                statusPlataText,
                                numeAbonament
                            );
                        }
                    }

                    context.SaveChanges();
                }

                IncarcaDateMixte();
                MessageBox.Show("Modificarea a fost salvată cu succes în baza de date și clientul a primit alertă în Inbox!", "Succes");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvarea statusului mixt: {ex.Message}", "Eroare DB");
            }
        }
    }

    // Clasa DTO Unificată care previne erorile de Id=0 și elimină incompatibilitățile de tip
    public class GestiuneElementUI
    {
        public int IdReal { get; set; }
        public string TipElement { get; set; }
        public string Username { get; set; }
        public string NumeComplet { get; set; }
        public string NumeOferta { get; set; }
        public string DataOraText { get; set; }
        public DateTime DataAsDateTime { get; set; }
        public string AntrenorText { get; set; }
        public string StatusText { get; set; }
        public bool EsteAbonamentPur { get; set; }
    }
}