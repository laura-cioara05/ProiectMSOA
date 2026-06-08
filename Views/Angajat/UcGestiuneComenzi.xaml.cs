using GymFitnessSystem.Data;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymFitnessSystem.Services;

namespace GymFitnessSystem.Views.Angajat
{
    public partial class UcGestiuneComenzi : UserControl
    {
        public UcGestiuneComenzi()
        {
            InitializeComponent();
            IncarcaComenzi();
        }

        private void IncarcaComenzi()
        {
            try
            {
                using (var context = new GymContext())
                {
                    string textCautat = txtCautareComenzi.Text.Trim().ToLower();

                    // Încărcăm mai întâi toate comenzile brute, forțând citirea directă a datelor
                    var query = context.Comenzi
                        .Include(c => c.Activitate)
                            .ThenInclude(a => a.Utilizator)
                        .Include(c => c.Activitate)
                            .ThenInclude(a => a.Oferta)
                        .AsNoTracking() // Împiedică EF să blocheze obiectele din cauza moștenirii duble
                        .AsQueryable();

                    if (!string.IsNullOrEmpty(textCautat))
                    {
                        query = query.Where(c => c.Activitate.Utilizator.NumeComplet.ToLower().Contains(textCautat) ||
                                                 c.Activitate.Utilizator.Username.ToLower().Contains(textCautat));
                    }

                    var listaComenzi = query.OrderBy(c => c.StatusLivrare).ToList();

                    // Forțăm manual popularea proprietății Id a obiectului Comanda
                    // cu ID-ul real extras din tabela părinte ActivitateBase, dacă EF l-a lăsat 0
                    foreach (var comanda in listaComenzi)
                    {
                        if (comanda.Id == 0 && comanda.Activitate != null)
                        {
                            comanda.Id = comanda.Activitate.Id;
                        }
                    }

                    dgComenzi.ItemsSource = listaComenzi;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea comenzilor: {ex.Message}", "Eroare date");
            }
        }

        private void TxtCautareComenzi_TextChanged(object sender, TextChangedEventArgs e)
        {
            IncarcaComenzi();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtCautareComenzi.Clear();
            IncarcaComenzi();
        }

        private void BtnPregatit_Click(object sender, RoutedEventArgs e)
        {
            ModificaStatusComanda(StatusComanda.LivratLaReceptie);
        }

        private void BtnRidicata_Click(object sender, RoutedEventArgs e)
        {
            ModificaStatusComanda(StatusComanda.Ridicata);
        }

        private void BtnAnuleaza_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Sigur vrei să anulezi această comandă?", "Confirmare", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                ModificaStatusComanda(StatusComanda.Anulata);
            }
        }

        private void ModificaStatusComanda(StatusComanda noulStatus)
        {
            var comandaSelectata = dgComenzi.SelectedItem as Comanda;
            if (comandaSelectata == null)
            {
                MessageBox.Show("Te rog selectează o comandă din tabel mai întâi!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idReal = comandaSelectata.Id;

            if (idReal == 0)
            {
                MessageBox.Show("Eroare gravă: ID-ul comenzii selectate este în continuare 0 în memorie!", "Eroare Date", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new GymContext())
                {
                    // Căutăm comanda în mod unic folosind ID-ul corectat+ Include pentru Oferta pentru a prelua numele produsului/serviciului
                    var comandaDb = context.Comenzi
                        .Include(c => c.Activitate)
                        .ThenInclude(a => a.Oferta)
                        .FirstOrDefault(c => c.Id == idReal);

                    if (comandaDb == null)
                    {
                        MessageBox.Show($"Comanda cu ID-ul {idReal} nu a fost găsită în baza de date!", "Eroare Identificare");
                        return;
                    }

                    // Actualizăm starea livrării în tabela [Comenzi]
                    comandaDb.StatusLivrare = noulStatus;

                    // Căutăm înregistrarea corelată din tabela [Plati] folosind ID-ul activității
                    var plataDb = context.Plati.FirstOrDefault(p => p.ActivitateId == idReal);

                    //  Aplicăm logica de business pentru actualizarea tabelelor prin SWITCH
                    switch (noulStatus)
                    {
                        case StatusComanda.LivratLaReceptie:
                            break;

                        case StatusComanda.Ridicata:
                            if (plataDb != null)
                            {
                                plataDb.Status = StatusPlata.Platit;

                                if (plataDb.Metoda == MetodaPlata.Cash)
                                {
                                    plataDb.DataPlatii = DateTime.Now;
                                }
                            }
                            break;

                        case StatusComanda.Anulata:
                            if (plataDb != null)
                            {
                                plataDb.Status = StatusPlata.Anulat;
                            }
                            break;

                        case StatusComanda.InProcesare:
                            break;
                    }

                    
                    // CONEXIUNE INBOX: Trimitem notificarea înainte de SaveChanges
                   
                    if (comandaDb.Activitate != null)
                    {
                        int idClient = comandaDb.Activitate.UtilizatorId;
                        string numeProdus = comandaDb.Activitate.Oferta?.Nume ?? "Produs Magazin";
                        string metodaPlataText = plataDb != null ? plataDb.Metoda.ToString() : "Nespecificată";

                        ServiciuNotificari.TrimiteNotificareStatus(
                            idClient,
                            "COMANDA",
                            noulStatus.ToString(),
                            numeProdus,
                            metodaPlataText
                        );
                    }


                    // Salvăm modificările efectuate în mod atomic
                    context.SaveChanges();
                }

                // Reîncărcăm datele în tabel
                IncarcaComenzi();
                MessageBox.Show($"Statusul comenzii a fost schimbat în [{noulStatus}].", "Succes");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la modificarea statusurilor: {ex.Message}\nDetalii: {ex.InnerException?.Message}", "Eroare SQL");
            }
        }
    }
}