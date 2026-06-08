using GymFitnessSystem.Models.Produs_Serviciu;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GymFitnessSystem.Views.Admin
{
    public partial class AdaugaProdusWindow : Window
    {
        public Produs ProdusCreat { get; private set; }

        public AdaugaProdusWindow()
        {
            InitializeComponent();
        }

        private void Salveaza_Click(object sender, RoutedEventArgs e)
        {
            string erori = "";

            if (string.IsNullOrWhiteSpace(txtNume.Text)) erori += "• Introduceți numele produsului.\n";
            if (!decimal.TryParse(txtPret.Text, out decimal pret) || pret < 0) erori += "• Prețul trebuie să fie un număr valid pozitiv.\n";
            if (!int.TryParse(txtStoc.Text, out int stoc) || stoc < 0) erori += "• Stocul inițial nu poate fi negativ.\n";

            if (!string.IsNullOrEmpty(erori))
            {
                MessageBox.Show(erori, "Eroare Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Luăm categoria selectată din ComboBox
            string categorieSelectata = (cmbCategorieProdus.SelectedItem as ComboBoxItem)?.Content.ToString();

            ProdusCreat = new Produs
            {
                Nume = txtNume.Text.Trim(),
                Producator = txtProducator.Text.Trim(),
                Pret = pret,
                Stoc = stoc,
                // Acum salvăm DOAR categoria direct în câmpul Descriere, alte texte suplimentare
                Descriere = categorieSelectata
            };

            this.DialogResult = true;
            this.Close();
        }



        private void Anuleaza_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}