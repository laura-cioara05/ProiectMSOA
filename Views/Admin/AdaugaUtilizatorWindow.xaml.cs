using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GymFitnessSystem.Models;

namespace GymFitnessSystem.Views.Admin
{
    public partial class AdaugaUtilizatorWindow : Window
    {
        // Aceasta va fi instanța pe care o legăm de interfață
        private Utilizator _utilizatorInLucru;

        public Utilizator UtilizatorCreat { get; private set; }

        public AdaugaUtilizatorWindow()
        {
            InitializeComponent();

            // Instanțiem un utilizator gol cu valori implicite
            _utilizatorInLucru = new Utilizator
            {
                DataCreare = DateTime.Now,
                EsteActiv = true
            };

            // Îi spunem ferestrei că TextBox-urile se leagă de acest obiect
            this.DataContext = _utilizatorInLucru;

            cmbRol.ItemsSource = Enum.GetValues(typeof(RolUtilizator));
            cmbRol.SelectedIndex = 2; // Client implicit
        }

        private void Salveaza_Click(object sender, RoutedEventArgs e)
        {
            // Preluăm rolul selectat din ComboBox
            _utilizatorInLucru.Rol = (RolUtilizator)cmbRol.SelectedItem;

            // Colectăm mesajele de eroare specifice din IDataErrorInfo
            string mesajEroare = "";

            // Luăm erorile pentru fiecare proprietate în parte
            string errUsername = _utilizatorInLucru[nameof(Utilizator.Username)];
            string errNume = _utilizatorInLucru[nameof(Utilizator.NumeComplet)]; 
            string errEmail = _utilizatorInLucru[nameof(Utilizator.Email)];
            string errParola = _utilizatorInLucru[nameof(Utilizator.Parola)];
            string errTelefon = _utilizatorInLucru[nameof(Utilizator.Telefon)];

            // Dacă o proprietate întoarce un text, înseamnă că e invalidă
            if (!string.IsNullOrEmpty(errUsername)) mesajEroare += $"• {errUsername}\n";
            if (!string.IsNullOrEmpty(errNume)) mesajEroare += $"• {errNume}\n";
            if (!string.IsNullOrEmpty(errEmail)) mesajEroare += $"• {errEmail}\n";
            if (!string.IsNullOrEmpty(errParola)) mesajEroare += $"• {errParola}\n";
            if (!string.IsNullOrEmpty(errTelefon)) mesajEroare += $"• {errTelefon}\n";

            // Dacă s-a acumulat vreo eroare, o afișăm detaliat
            if (!string.IsNullOrEmpty(mesajEroare))
            {
                MessageBox.Show($"Nu poți salva utilizatorul! Corectează următoarele probleme:\n\n{mesajEroare}",
                                "Eroare Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Oprim salvarea
            }

            // salvăm rezultatul final și închidem
            UtilizatorCreat = _utilizatorInLucru;
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