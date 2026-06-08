using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymFitnessSystem.Services;

namespace GymFitnessSystem.Views.Client
{
    public partial class UcInbox : UserControl
    {
        private int _idClientLogat;

        public UcInbox()
        {
            InitializeComponent();

            // Preluăm ID-ul clientului curent logat din sesiunea aplicației tale
            
            _idClientLogat = UserSession.LoggedInUserId;

            IncarcaNotificari();
        }

        private void IncarcaNotificari()
        {
            try
            {
                using (var context = new GymContext())
                {
                    // Luăm notificările clientului ordonate de la cea mai nouă la cea mai veche
                    var lista = context.Notificari
                        .Where(n => n.UtilizatorId == _idClientLogat)
                        .OrderByDescending(n => n.DataTrimitere)
                        .ToList();

                    lbNotificari.ItemsSource = lista;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea inbox-ului: {ex.Message}");
            }
        }

        // Când clientul dă click pe o notificare, o marcăm automat ca fiind CITITĂ în baza de date
        private void LbNotificari_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectata = lbNotificari.SelectedItem as Notificare;
            if (selectata == null || selectata.EsteCitit) return;

            try
            {
                using (var context = new GymContext())
                {
                    var notificareDb = context.Notificari.FirstOrDefault(n => n.Id == selectata.Id);
                    if (notificareDb != null)
                    {
                        notificareDb.EsteCitit = true;
                        context.SaveChanges();
                    }
                }

                // Reîncărcăm ca să dispară textul îngroșat (Bold)
                IncarcaNotificari();
            }
            catch { }
        }

        // Buton pentru curățare rapidă
        private void BtnMarcheazaCitit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new GymContext())
                {
                    var necitite = context.Notificari.Where(n => n.UtilizatorId == _idClientLogat && !n.EsteCitit).ToList();
                    foreach (var n in necitite)
                    {
                        n.EsteCitit = true;
                    }
                    context.SaveChanges();
                }
                IncarcaNotificari();
            }
            catch { }
        }
    }
}