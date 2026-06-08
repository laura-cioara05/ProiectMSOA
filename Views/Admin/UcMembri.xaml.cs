using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using GymFitnessSystem.Services;

namespace GymFitnessSystem.Views.Admin
{
    public partial class UcMembri : UserControl
    {
        private readonly GymContext _context;
        private readonly IRepository<Utilizator> _utilizatorRepository;
        private List<Utilizator> _toțiUtilizatorii = new List<Utilizator>();

        public UcMembri()
        {
            InitializeComponent();

            _context = new GymContext();
            _utilizatorRepository = new GenericRepository<Utilizator>(_context);

            colRol.ItemsSource = Enum.GetValues(typeof(RolUtilizator));

            _ = IncarcaMembri();
        }

        private async Task IncarcaMembri()
        {
            try
            {
                var membri = await _utilizatorRepository.GetAllAsync();
                _toțiUtilizatorii = membri.ToList();
                dgUtilizatori.ItemsSource = _toțiUtilizatorii;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea membrilor: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltruRol_Checked(object sender, RoutedEventArgs e)
        {
            if (_toțiUtilizatorii == null || dgUtilizatori == null) return;

            if (sender is RadioButton rb && rb.Tag != null)
            {
                string tag = rb.Tag.ToString();

                if (tag == "Toți")
                {
                    dgUtilizatori.ItemsSource = _toțiUtilizatorii;
                }
                else
                {
                    dgUtilizatori.ItemsSource = _toțiUtilizatorii
                        .Where(u => u.Rol.ToString() == tag)
                        .ToList();
                }
            }
        }

        private void dgUtilizatori_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            // Ne asigurăm că prindem obiectul modificat
            if (e.Row.Item is Utilizator u)
            {
                string mesajEroare = "";

                //  Validare Username
                if (string.IsNullOrWhiteSpace(u.Username))
                {
                    mesajEroare += "• Câmpul 'Username' nu poate fi gol.\n";
                }

                //  Validare Nume Complet
                if (string.IsNullOrWhiteSpace(u.NumeComplet))
                {
                    mesajEroare += "• Câmpul 'Nume Complet' nu poate fi gol.\n";
                }

                //  Validare Email (să conțină @ și un domeniu valid ca .com, .ro etc.)
                if (string.IsNullOrWhiteSpace(u.Email))
                {
                    mesajEroare += "• Câmpul 'Email' nu poate fi gol.\n";
                }
                else if (!u.Email.Contains("@") || (!u.Email.EndsWith(".com") && !u.Email.EndsWith(".ro") && !u.Email.EndsWith(".net")))
                {
                    mesajEroare += "• Emailul introdus nu este valid (trebuie să conțină '@' și să se termine în .com, .ro sau .net).\n";
                }

                //  Validare Telefon (Trebuie să fie format doar din cifre și să aibă exact 10 caractere)
                if (string.IsNullOrWhiteSpace(u.Telefon))
                {
                    mesajEroare += "• Câmpul 'Telefon' nu poate fi gol.\n";
                }
                else
                {
                    // Curățăm eventuale spații accidentale
                    string telCurat = u.Telefon.Trim();
                    bool doarCifre = long.TryParse(telCurat, out _);

                    if (telCurat.Length != 10 || !doarCifre)
                    {
                        mesajEroare += "• Numărul de telefon trebuie să conțină EXACT 10 cifre.\n";
                    }
                }

                //  Validare Parolă (minim 3 caractere)
                if (string.IsNullOrWhiteSpace(u.Parola) || u.Parola.Length < 3)
                {
                    mesajEroare += "• Parola trebuie să aibă cel puțin 3 caractere.\n";
                }

                // Dacă s-a acumulat vreo eroare, blocăm salvarea
                if (!string.IsNullOrEmpty(mesajEroare))
                {
                    MessageBox.Show($"Modificările NU au fost salvate deoarece s-au găsit următoarele erori:\n\n{mesajEroare}",
                                    "Eroare Validare Date",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                    e.Cancel = true; // Oprește tabelul din a accepta datele greșite
                    return;          
                }

                // Dacă codul a ajuns aici, înseamnă că toate datele sunt perfect valide
                try
                {
                    using (var context = new GymContext())
                    {
                        context.Utilizatori.Update(u);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare critică la salvarea în baza de date: {ex.Message}", "Eroare SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
            }
        }

        private void dgUtilizatori_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Forțăm DataGrid-ul să facă Commit la rând imediat ce o celulă de tip CheckBox și-a schimbat starea
            if (e.Column is DataGridCheckBoxColumn)
            {
                var dataGrid = sender as DataGrid;
                if (dataGrid != null)
                {
                    // Rulăm asincron un mic update ca să nu blocăm thread-ul de UI în mijlocul editării
                    Dispatcher.BeginInvoke(new Action(() => dataGrid.CommitEdit(DataGridEditingUnit.Row, true)), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }
        private async void AdaugaUtilizator_Click(object sender, RoutedEventArgs e)
        {
            AdaugaUtilizatorWindow dialog = new AdaugaUtilizatorWindow();
            dialog.Owner = Window.GetWindow(this); // Corect, păstrează centrarea ferestrei părinte

            // Verificăm strict dacă utilizatorul a trecut de validări și a apăsat "Salvează"
            if (dialog.ShowDialog() == true)
            {
                var nouUtilizator = dialog.UtilizatorCreat;

                // O verificare de siguranță extra în caz că obiectul a venit gol din dialog
                if (nouUtilizator == null) return;

                try
                {
                    // Salvare asincronă în baza de date (Foarte bine că e cu await!)
                    await _utilizatorRepository.AddAsync(nouUtilizator);

                    // Adăugăm în lista locală din memorie
                    _toțiUtilizatorii.Add(nouUtilizator);

                    // se reîmprospăteaza instant
                    dgUtilizatori.Items.Refresh();

                    MessageBox.Show($"Utilizatorul {nouUtilizator.Username} a fost creat cu succes!",
                                    "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la salvarea în baza de date: {ex.Message}",
                                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SchimbaStare_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button butonStare && butonStare.CommandParameter is int idUtilizator)
            {
                var utilizator = _toțiUtilizatorii.FirstOrDefault(u => u.Id == idUtilizator);

                if (utilizator != null)
                {
                    utilizator.EsteActiv = !utilizator.EsteActiv;

                    try
                    {
                        await _utilizatorRepository.UpdateAsync(utilizator);

                        dgUtilizatori.ItemsSource = null;
                        dgUtilizatori.ItemsSource = _toțiUtilizatorii;

                        string stare = utilizator.EsteActiv ? "activat" : "dezactivat";
                        MessageBox.Show($"Contul utilizatorului {utilizator.Username} a fost {stare}!", "Status Actualizat", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        utilizator.EsteActiv = !utilizator.EsteActiv;
                        MessageBox.Show($"Nu s-a putut schimba starea în baza de date: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}