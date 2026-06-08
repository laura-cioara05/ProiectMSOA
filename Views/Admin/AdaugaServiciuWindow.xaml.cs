using GymFitnessSystem.Models.Produs_Serviciu;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GymFitnessSystem.Views.Admin
{
    public partial class AdaugaServiciuWindow : Window
    {
        public Serviciu ServiciuCreat { get; private set; }

        public AdaugaServiciuWindow()
        {
            InitializeComponent();
        }

        private void Salveaza_Click(object sender, RoutedEventArgs e)
        {
            string erori = "";

            if (string.IsNullOrWhiteSpace(txtNume.Text)) erori += "• Introduceți denumirea.\n";
            if (!decimal.TryParse(txtPret.Text, out decimal pret) || pret < 0) erori += "• Prețul trebuie să fie un număr valid pozitiv.\n";
            if (!int.TryParse(txtDurata.Text, out int durata) || durata <= 0) erori += "• Durata trebuie să fie de minimum 1 zi.\n";

            if (!string.IsNullOrEmpty(erori))
            {
                MessageBox.Show(erori, "Eroare Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Luăm categoria selectată din ComboBox-ul pe care l-am pus anterior
            string categorieSelectata = (cmbCategorieServiciu.SelectedItem as ComboBoxItem)?.Content.ToString();

            ServiciuCreat = new Serviciu
            {
                Nume = txtNume.Text.Trim(),
                Pret = pret,
                DurataZile = durata,
                NecesitaProgramare = chkProgramare.IsChecked ?? false,
                // Salvăm structurat: "Categorie|Descrierea efectivă scrisa de admin"
                Descriere = $"{categorieSelectata}|{txtDescriere.Text.Trim()}"
            };

            this.DialogResult = true;
            this.Close();
        }

        private void CmbCategorieServiciu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Verificăm de siguranță să nu fie controlul încă neinițializat la pornirea ferestrei
            if (chkProgramare == null) return;

            var itemSelectat = (cmbCategorieServiciu.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Dacă NU este abonament clasic, înseamnă că e Yoga/Aerobic/Antrenor -> au nevoie de programare obligatorie
            if (itemSelectat != "Abonament")
            {
                chkProgramare.IsChecked = true;
                chkProgramare.IsEnabled = false; // Îl blocăm pe True ca să nu poată face prostii operatorul
            }
            else
            {
                chkProgramare.IsChecked = false;
                chkProgramare.IsEnabled = true;  // Îl lăsăm editabil pentru abonamente speciale
            }
        }
        private void Anuleaza_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}