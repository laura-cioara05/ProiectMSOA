using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GymFitnessSystem.Views.Client
{
    public partial class UcProfil : UserControl
    {
        private Utilizator _utilizatorCurent;

        public UcProfil()
        {
            InitializeComponent();
            IncarcaDateProfil();
            IncarcaIstoricActivitate();
        }

        // ÎNCĂRCARE DATE PERSONALE IN TEXTBOX-URI
        private void IncarcaDateProfil()
        {
            int idClient = UserSession.LoggedInUserId;
            try
            {
                using (var context = new GymContext())
                {
                    _utilizatorCurent = context.Utilizatori.FirstOrDefault(u => u.Id == idClient);

                    if (_utilizatorCurent != null)
                    {
                        txtUsername.Text = _utilizatorCurent.Username;
                        txtNumeComplet.Text = _utilizatorCurent.NumeComplet;
                        txtEmail.Text = _utilizatorCurent.Email;
                        txtTelefon.Text = _utilizatorCurent.Telefon;
                        txtParola.Text = _utilizatorCurent.Parola;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea profilului: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //  SALVARE MODIFICĂRI PROFIL (FĂRĂ ROL, ID SAU ACTIVITATE CONT)
        private void SalveazaProfil_Click(object sender, RoutedEventArgs e)
        {
            if (_utilizatorCurent == null) return;

            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtNumeComplet.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Câmpurile Username, Nume Complet și Email sunt obligatorii!", "Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new GymContext())
                {
                    var utilizatorDb = context.Utilizatori.FirstOrDefault(u => u.Id == _utilizatorCurent.Id);
                    if (utilizatorDb != null)
                    {
                        utilizatorDb.Username = txtUsername.Text.Trim();
                        utilizatorDb.NumeComplet = txtNumeComplet.Text.Trim();
                        utilizatorDb.Email = txtEmail.Text.Trim();
                        utilizatorDb.Telefon = txtTelefon.Text.Trim();
                        utilizatorDb.Parola = txtParola.Text; // Păstrăm simplu, conform cerinței

                        context.SaveChanges();
                        MessageBox.Show("Datele profilului au fost actualizate cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvarea datelor: {ex.Message}", "Eroare SQL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //  ÎNCĂRCARE ISTORIC ACTIVITĂȚI TERMINATE (TRAGE DOAR STĂRILE FINALE)
        private void IncarcaIstoricActivitate()
        {
            int idClient = UserSession.LoggedInUserId;

            try
            {
                using (var context = new GymContext())
                {
                    // A. ISTORIC PROGRAMĂRI (Doar Prezent sau Anulat)
                    var istoriculProgramari = context.Programari
                        .Include(p => p.Activitate)
                        .Include(p => p.Antrenor)
                        .Where(p => p.UtilizatorId == idClient && (p.Status == StatusPrezenta.Prezent || p.Status == StatusPrezenta.Anulat))
                        .ToList();

                    dgIstoricProgramari.ItemsSource = istoriculProgramari.Select(p => {
                        var plata = context.Plati.FirstOrDefault(pl => pl.ActivitateId == p.Activitate.Id);
                        string mPlata = (plata != null && (plata.Metoda.ToString() == "1" || plata.Metoda.ToString().Contains("Card"))) ? "Card" : "Cash";

                        // Folosim clasa dedicată de mai jos pentru a mapa proprietățile sigur
                        return new IstoricProgramareUI
                        {
                            NumeServiciu = context.Servicii.FirstOrDefault(s => s.Id == p.Activitate.OfertaId)?.Nume ?? "Ședință Fitness",
                            DataOraText = p.DataOraProgramata.ToString("dd.MM.yyyy HH:mm"),
                            AntrenorText = p.Antrenor != null ? p.Antrenor.NumeComplet : "Fără antrenor",
                            MetodaPlata = mPlata,
                            StatusText = p.Status.ToString() // Va scoate string-ul exact "Prezent" sau "Anulat" pentru DataTrigger
                        };
                    }).ToList();


                    // B. ISTORIC COMENZI BAR (Doar Ridicata sau Anulata)
                    var istoriculComenzi = context.Comenzi
                        .Include(c => c.Activitate)
                        .Where(c => c.Activitate.UtilizatorId == idClient && (c.StatusLivrare == StatusComanda.Ridicata || c.StatusLivrare == StatusComanda.Anulata))
                        .ToList();

                    dgIstoricComenzi.ItemsSource = istoriculComenzi.Select(c => {
                        var oferta = context.Oferte.FirstOrDefault(o => o.Id == c.Activitate.OfertaId);
                        var plata = context.Plati.FirstOrDefault(pl => pl.ActivitateId == c.Activitate.Id);
                        string mPlata = (plata != null && (plata.Metoda.ToString() == "1" || plata.Metoda.ToString().Contains("Card"))) ? "Card" : "Cash";

                        return new IstoricComandaUI
                        {
                            NumeProdus = oferta != null ? oferta.Nume : "Produs Bar",
                            DetaliiCantitate = $"{c.Cantitate} buc. | Total: {(oferta != null ? oferta.Pret * c.Cantitate : 0):F2} RON",
                            MetodaPlata = mPlata,
                            StatusText = c.StatusLivrare.ToString() // Va scoate string-ul exact "Ridicata" sau "Anulata" pentru DataTrigger
                        };
                    }).ToList();

                    // ==================================================
                    // C. ISTORIC ABONAMENTE PURE (Din ActivitateBase + Plati)
                    // ==================================================
                    var idUriComenzi = context.Comenzi.Select(c => c.Id).ToList();
                    var idUriProgramari = context.Programari.Select(p => p.Id).ToList();

                    // Citim activitățile de bază ale clientului excluzând tabelele derivate
                    var activitatiIstoricBrute = context.Activitati
                        .Include(a => a.Oferta)
                        .Where(a => a.UtilizatorId == idClient
                                 && !idUriComenzi.Contains(a.Id)
                                 && !idUriProgramari.Contains(a.Id))
                        .ToList();

                    var listaIstoricAbonamente = activitatiIstoricBrute.Select(a =>
                    {
                        var plata = context.Plati.FirstOrDefault(p => p.ActivitateId == a.Id);

                        // Reținem doar plățile finalizate: Platit sau Anulat
                        if (plata == null || (plata.Status != StatusPlata.Platit && plata.Status != StatusPlata.Anulat))
                            return null;

                        return new IstoricAbonamentUI
                        {
                            NumeAbonament = a.Oferta != null ? a.Oferta.Nume : "Abonament / Serviciu Sală",
                            ValoareText = $"{plata.Suma:F2} RON",
                            MetodaPlata = plata.Metoda.ToString(),
                            StatusText = plata.Status.ToString() // Se trimite string exact "Platit" sau "Anulat" spre XAML Triggers
                        };
                    })
                    .Where(item => item != null)
                    .ToList();

                    dgIstoricAbonamente.ItemsSource = listaIstoricAbonamente;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea istoricului: {ex.Message}", "Eroare Date", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // CLASE DTO AJUTĂTOARE PENTRU BINDING STABIL ÎN DATA GRID
    public class IstoricProgramareUI
    {
        public string NumeServiciu { get; set; }
        public string DataOraText { get; set; }
        public string AntrenorText { get; set; }
        public string MetodaPlata { get; set; }
        public string StatusText { get; set; }
    }

    public class IstoricComandaUI
    {
        public string NumeProdus { get; set; }
        public string DetaliiCantitate { get; set; }
        public string MetodaPlata { get; set; }
        public string StatusText { get; set; }
    }

    public class IstoricAbonamentUI
    {
        public string NumeAbonament { get; set; }
        public string ValoareText { get; set; }
        public string MetodaPlata { get; set; }
        public string StatusText { get; set; }
    }
}
