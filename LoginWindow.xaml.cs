using GymFitnessSystem.Models;
using GymFitnessSystem.Services;
using GymFitnessSystem.ViewModels;
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

namespace GymFitnessSystem.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel = new LoginViewModel();

        public LoginWindow()
        {
            InitializeComponent();
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Luăm datele din interfață
            string user = txtUsername.Text;
            string pass = txtPassword.Password;

            // Verificăm în baza de date prin ViewModel
            var utilizatorGasit = _viewModel.VerificaLogin(user, pass);

            if (utilizatorGasit != null)
            {
                if (!utilizatorGasit.EsteActiv)
                {
                    MessageBox.Show("Ne pare rău, dar acest cont a fost dezactivat de către un administrator.\n" +
                                    "Pentru detalii sau deblocare, te rugăm să contactezi recepția sălii.",
                                    "Cont Inactiv",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                    // Oprim procesul de logare aici! Nu mai creăm sesiune, nu mai deschidem ferestre.
                    return;
                }

                UserSession.LoggedInUserId = utilizatorGasit.Id;
                UserSession.Username = utilizatorGasit.Username;

                // Switch-ul decide ce fereastră deschidem bazat pe coloana "Rol" din SQL
                switch (utilizatorGasit.Rol)
                {
                    case RolUtilizator.Administrator:
                        GymFitnessSystem.Views.Admin.AdminWindow adminWin = new GymFitnessSystem.Views.Admin.AdminWindow();
                        adminWin.Show();
                        this.Close(); // Închide fereastra de Login
                        break;

                    case RolUtilizator.Angajat:
                        GymFitnessSystem.Views.Angajat.AngajatWindow angajatWin = new GymFitnessSystem.Views.Angajat.AngajatWindow();
                        angajatWin.Show();
                        this.Close(); // Închide fereastra de Login
                        break;

                    case RolUtilizator.Client:
                    
                        GymFitnessSystem.Views.Client.ClientWindow clientWin = new GymFitnessSystem.Views.Client.ClientWindow();
                        clientWin.Show();
                        this.Close(); // Închidem ecranul de login
                        break;

                    default:
                        MessageBox.Show("Eroare: Rolul utilizatorului nu este recunoscut.");
                        break;
                }
            }
            else
            {
                MessageBox.Show("Eroare: Utilizator sau parolă incorectă!");
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 1. Colectăm datele din TextBox-uri
            string user = txtNewUser.Text.Trim();
            string pass = txtNewPass.Password;
            string mail = txtNewEmail.Text.Trim();
            string nume = txtNewNume.Text.Trim();
            string tel = txtNewTel.Text.Trim();

            string mesajEroare = "";

            // Definim culorile pentru validare (Roșu pentru eroare, Gri-ul tău original din XAML pentru corect)
            var culoareEroare = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")); // Roșu modern
            var culoareOk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));      // Gri-ul tău inițial

            // Părinții TextBox-urilor sunt elementele de tip Border care au proprietatea BorderBrush.
            // Luăm marginile fiecăruia ca să le putem schimba culoarea.
            var borderNume = (Border)txtNewNume.Parent;
            var borderUser = (Border)txtNewUser.Parent;
            var borderEmail = (Border)txtNewEmail.Parent;
            var borderTel = (Border)txtNewTel.Parent;
            var borderPass = (Border)txtNewPass.Parent;

            //  Validare Nume Complet
            if (string.IsNullOrWhiteSpace(nume))
            {
                mesajEroare += "• Câmpul 'Nume Complet' nu poate fi gol.\n";
                borderNume.BorderBrush = culoareEroare;
            }
            else
            {
                borderNume.BorderBrush = culoareOk;
            }

            //  Validare Username
            if (string.IsNullOrWhiteSpace(user))
            {
                mesajEroare += "• Câmpul 'Username' nu poate fi gol.\n";
                borderUser.BorderBrush = culoareEroare;
            }
            else
            {
                borderUser.BorderBrush = culoareOk;
            }

            //  Validare Email
            if (string.IsNullOrWhiteSpace(mail) || !mail.Contains("@") ||
                (!mail.EndsWith(".com") && !mail.EndsWith(".ro") && !mail.EndsWith(".net")))
            {
                mesajEroare += "• Emailul nu este valid (trebuie să conțină '@' și să se termine în .com, .ro sau .net).\n";
                borderEmail.BorderBrush = culoareEroare;
            }
            else
            {
                borderEmail.BorderBrush = culoareOk;
            }

            //  Validare Telefon
            bool doarCifre = long.TryParse(tel, out _);
            if (string.IsNullOrWhiteSpace(tel) || tel.Length != 10 || !doarCifre)
            {
                mesajEroare += "• Numărul de telefon trebuie să conțină EXACT 10 cifre.\n";
                borderTel.BorderBrush = culoareEroare;
            }
            else
            {
                borderTel.BorderBrush = culoareOk;
            }

            //  Validare Parolă
            if (string.IsNullOrWhiteSpace(pass) || pass.Length < 3)
            {
                mesajEroare += "• Parola trebuie să aibă cel puțin 3 caractere.\n";
                borderPass.BorderBrush = culoareEroare;
            }
            else
            {
                borderPass.BorderBrush = culoareOk;
            }

            // Dacă s-a acumulat vreo eroare, blocăm procesul și afișăm mesajul
            if (!string.IsNullOrEmpty(mesajEroare))
            {
                MessageBox.Show($"Înregistrarea NU a putut fi procesată din cauza următoarelor erori:\n\n{mesajEroare}",
                                "Eroare Validare Date",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Dacă totul e ok în interfață, verificăm unicitatea în baza de date prin ViewModel
            bool succes = _viewModel.InregistrareUtilizator(user, pass, mail, nume, tel);

            if (succes)
            {
                MessageBox.Show("Contul a fost creat cu succes! Te poți loga.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                // Curățăm câmpurile
                txtNewUser.Clear();
                txtNewPass.Clear();
                txtNewEmail.Clear();
                txtNewNume.Clear();
                txtNewTel.Clear();

                // Resetăm toate bordurile înapoi la gri normal pentru următoarea utilizare
                borderNume.BorderBrush = culoareOk;
                borderUser.BorderBrush = culoareOk;
                borderEmail.BorderBrush = culoareOk;
                borderTel.BorderBrush = culoareOk;
                borderPass.BorderBrush = culoareOk;

                SwitchToLogin_Click(sender, e);
            }
            else
            {
                // Dacă eroarea vine din DB (deja există user/email), colorăm ambele câmpuri suspecte în roșu
                borderUser.BorderBrush = culoareEroare;
                borderEmail.BorderBrush = culoareEroare;

                MessageBox.Show("Eroare: Username-ul sau Email-ul există deja în sistem.", "Eroare Unicitate", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwitchToRegister_Click(object sender, RoutedEventArgs e)
        {
            PanelLogin.Visibility = Visibility.Collapsed;
            btnLogin.IsDefault = false; // Dezactivăm Enter pe login

            PanelRegister.Visibility = Visibility.Visible;
            btnRegister.IsDefault = true; // Mutăm Enter pe înregistrare
        }

        private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            PanelRegister.Visibility = Visibility.Collapsed;
            btnRegister.IsDefault = false; // Dezactivăm Enter pe înregistrare

            PanelLogin.Visibility = Visibility.Visible;
            btnLogin.IsDefault = true; // Mutăm Enter pe login
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
