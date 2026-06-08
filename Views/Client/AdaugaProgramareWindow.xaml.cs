using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static GymFitnessSystem.Views.Client.UcShop;

namespace GymFitnessSystem.Views.Client
{
    public partial class AdaugaProgramareWindow : Window
    {
        private ElementMagazin _item;
        private bool _necesitaAntrenorSelector = false;

        public AdaugaProgramareWindow(ElementMagazin item)
        {
            InitializeComponent();
            _item = item;

            txtNumeServiciu.Text = item.DenumireAfisare;
            txtTotal.Text = $"{item.PretAfisare:F2} RON";

            string categorieCurata = item.CategorieAfisare?.Trim() ?? "";

            //  Caz: ABONAMENT
            if (categorieCurata.Contains("Abonament", StringComparison.OrdinalIgnoreCase))
            {
                pnlProgramare.Visibility = Visibility.Collapsed;
                chkOptiuneAntrenor.Visibility = Visibility.Collapsed;
            }
            //  Caz: ANTRENOR PERSONAL
            else if (categorieCurata.Contains("Antrenor", StringComparison.OrdinalIgnoreCase))
            {
                pnlProgramare.Visibility = Visibility.Visible;
                chkOptiuneAntrenor.Visibility = Visibility.Visible; // Permitem alegerea antrenorului
                _necesitaAntrenorSelector = true;
                IncarcaAntrenoriDinDb();
            }
            // Caz: CLASA DE GRUP (sau alte servicii standard)
            else
            {
                pnlProgramare.Visibility = Visibility.Visible;
                chkOptiuneAntrenor.Visibility = Visibility.Collapsed; // Ascundem complet opțiunea de antrenor
                pnlSelectieAntrenor.Visibility = Visibility.Collapsed;
            }
        }

        private void IncarcaAntrenoriDinDb()
        {
            try
            {
                using (var context = new GymContext())
                {
                    // Filtrăm curat folosind proprietățile tale reale din baza de date
                    var antrenori = context.Utilizatori
                        .Where(u => u.EsteAntrenor == true)
                        .Select(u => new { Id = u.Id, Nume = u.NumeComplet })
                        .ToList();

                    cmbAntrenori.ItemsSource = antrenori;
                    if (antrenori.Any())
                    {
                        cmbAntrenori.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea antrenorilor: {ex.Message}");
            }
        }

        private void ChkVreauAntrenor_Checked(object sender, RoutedEventArgs e)
        {
            pnlSelectieAntrenor.Visibility = Visibility.Visible;
        }

        private void ChkVreauAntrenor_Unchecked(object sender, RoutedEventArgs e)
        {
            pnlSelectieAntrenor.Visibility = Visibility.Collapsed;
        }

        private void BtnFinalizare_Click(object sender, RoutedEventArgs e)
        {
            string categorieCurata = _item.CategorieAfisare?.Trim() ?? "";
            bool esteAbonament = categorieCurata.Contains("Abonament", StringComparison.OrdinalIgnoreCase);

            if (!esteAbonament)
            {
                if (dpData.SelectedDate == null || cmbOra.SelectedItem == null)
                {
                    MessageBox.Show("Te rugăm să selectezi data și ora pentru programare!", "Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validare doar dacă suntem pe categoria Antrenor Personal și s-a bifat că se dorește unul
                if (_necesitaAntrenorSelector && chkVreauAntrenor.IsChecked == true && cmbAntrenori.SelectedItem == null)
                {
                    MessageBox.Show("Te rugăm să selectezi un antrenor din listă!", "Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            using (var context = new GymContext())
            {
                var selectedItem = cmbMetodaPlata.SelectedItem as ComboBoxItem;
                string metodaText = selectedItem?.Content.ToString() ?? "Cash";
                var metodaSelectata = (metodaText == "Card") ? MetodaPlata.Card : MetodaPlata.Cash;
                StatusPlata statusInitial = (metodaSelectata == MetodaPlata.Card) ? StatusPlata.Platit : StatusPlata.InAsteptare;

                ActivitateBase entitateActivitate;

                if (esteAbonament)
                {
                    entitateActivitate = new ActivitateBase
                    {
                        UtilizatorId = UserSession.LoggedInUserId,
                        OfertaId = _item.IdOferta,
                        PretAchitat = _item.PretAfisare,
                        DataInregistrare = DateTime.Now
                    };
                    context.Activitati.Add(entitateActivitate);
                }
                else
                {
                    var textOra = (cmbOra.SelectedItem as ComboBoxItem).Content.ToString();
                    DateTime dataOraCompleta = dpData.SelectedDate.Value.Add(TimeSpan.Parse(textOra));

                    int? idAntrenorSelectat = null;
                    // Salvăm antrenorul doar dacă suntem pe categoria potrivită ȘI utilizatorul a bifat că vrea
                    if (_necesitaAntrenorSelector && chkVreauAntrenor.IsChecked == true && cmbAntrenori.SelectedItem != null)
                    {
                        dynamic antrenorSelectat = cmbAntrenori.SelectedItem;
                        idAntrenorSelectat = antrenorSelectat.Id;
                    }

                    var programare = new Programare
                    {
                        UtilizatorId = UserSession.LoggedInUserId,
                        OfertaId = _item.IdOferta,
                        PretAchitat = _item.PretAfisare,
                        DataInregistrare = DateTime.Now,

                        DataOraProgramata = dataOraCompleta,
                        AntrenorId = idAntrenorSelectat, // Va fi ID sau NULL (pentru Clase de grup sau dacă nu vrea antrenor la ședință)
                        Status = 0
                    };

                    context.Programari.Add(programare);
                    entitateActivitate = programare;
                }

                var plata = new Plata
                {
                    Activitate = entitateActivitate,
                    Suma = _item.PretAfisare,
                    Metoda = metodaSelectata,
                    Status = statusInitial,
                    DataPlatii = DateTime.Now
                };
                context.Plati.Add(plata);

                context.SaveChanges();
            }

            MessageBox.Show("Achiziție realizată cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void BtnAnulare_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}