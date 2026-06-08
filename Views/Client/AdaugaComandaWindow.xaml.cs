using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Models.Produs_Serviciu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GymFitnessSystem.Views.Client
{
    public partial class AdaugaComandaWindow : Window
    {
        private readonly GymContext _db = new GymContext();
        private readonly int _currentUserId;
        private readonly Produs _produsSelectat;
        private int _cantitate = 1; // Variabilă privată de clasă

        public AdaugaComandaWindow(int userId, int ofertaId)
        {
            InitializeComponent();
            _currentUserId = userId;

            var oferta = _db.Oferte.Find(ofertaId);
            if (oferta is Produs produs)
            {
                _produsSelectat = produs;
                txtNumeProdus.Text = produs.Nume; // Setează numele produsului
                ActualizeazaTotal();
            }
        }

        private void ActualizeazaTotal()
        {
            // Folosim _cantitate (variabila clasei) și txtTotal (numele din XAML)
            decimal total = _produsSelectat.Pret * _cantitate;
            txtTotal.Text = $"{total:F2} RON"; // Am corectat numele txtTotal
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            _cantitate++;
            txtCantitate.Text = _cantitate.ToString();
            ActualizeazaTotal();
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (_cantitate > 1)
            {
                _cantitate--;
                txtCantitate.Text = _cantitate.ToString();
                ActualizeazaTotal();
            }
        }

        private void BtnAnulare_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnFinalizare_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtCantitate.Text, out int cantitate)) return;

            using (var context = new GymContext())
            {
                var produs = context.Produse.Find(_produsSelectat.Id);

                if (produs == null)
                {
                    MessageBox.Show("Produsul nu a fost găsit!");
                    return;
                }

                if (produs.Stoc < cantitate)
                {
                    MessageBox.Show("Stoc insuficient!");
                    return;
                }

                //  Scădem stocul
                produs.Stoc -= cantitate;

                // 2. Creăm obiectul Comanda
                var comanda = new Comanda
                {
                    UtilizatorId = _currentUserId,
                    OfertaId = produs.Id,
                    PretAchitat = produs.Pret * cantitate,
                    DataInregistrare = DateTime.Now,
                    Cantitate = cantitate,
                    StatusLivrare = StatusComanda.InProcesare
                };

                // Adăugăm comanda în context
                context.Comenzi.Add(comanda);

                // Identificăm metoda de plată din UI
                var selectedItem = cmbMetodaPlata.SelectedItem as ComboBoxItem;
                string metodaText = selectedItem?.Content.ToString() ?? "Cash";
                var metodaSelectata = (metodaText == "Card") ? MetodaPlata.Card : MetodaPlata.Cash;

                // Stabilim statusul plății
                StatusPlata statusInitial = (metodaSelectata == MetodaPlata.Card) ? StatusPlata.Platit : StatusPlata.InAsteptare;

                // Creăm plata legând-o direct de OBIECTUL comanda prin proprietatea de navigare, 
                // NU prin ID-ul numeric care încă nu s-a generat în memorie!
                var plata = new Plata
                {
                    Activitate = comanda, // <--- Legăm obiectele direct în memorie.
                    Suma = produs.Pret * cantitate,
                    Metoda = metodaSelectata,
                    Status = statusInitial,
                    DataPlatii = DateTime.Now
                };

                context.Plati.Add(plata);

                // Rulăm o SINGURĂ salvare la final pentru tot contextul.
                // EF Core va analiza graficul de entități, va insera Activitatea, va lua ID-ul, 
                // îl va pune în Comenzi și apoi îl va folosi automat și în Plati în mod corect
                context.SaveChanges();
            }

            MessageBox.Show("Comandă finalizată cu succes!");
            this.Close();
        }
    }
}