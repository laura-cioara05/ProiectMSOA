using System.Windows;

namespace GymFitnessSystem.Views.Admin
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            // Pagina de start implicită este Dashboard-ul
            ContentArea.Content = new UcDashboard();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new UcDashboard();
        }

        private void Membri_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new UcMembri();
        }

        private void Economic_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new UcManagement();
        }

        private void Setari_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new UcSetari();
        }
        private void Management_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new UcManagement();
        }

        private void Performanta_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new UcPerformanta();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}







































/*using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using GymFitnessSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace GymFitnessSystem.Views
{
    public partial class AdminWindow : Window
    {
        public ISeries[] Series { get; set; } = {
            new ColumnSeries<double> { Values = new double[] { 1200, 1500, 1100, 2000 }, Name = "Încasări (RON)" }
        };

        private SetariSistem _setariCurente;
        private readonly GymContext _context;
        private readonly IRepository<Utilizator> _utilizatorRepository;
        private List<Utilizator> _toțiUtilizatorii = new List<Utilizator>();

        public AdminWindow()
        {
            InitializeComponent();

            InitializeazaComboBoxOre();

            Loaded += async (s, e) => await IncarcaSiPopuleazaSetariSistemAsync();

            ChartIncasari.Series = Series;

            _context = new GymContext();
            _utilizatorRepository = new GenericRepository<Utilizator>(_context);

            colRol.ItemsSource = Enum.GetValues(typeof(RolUtilizator));
        }

        private void InitializeazaComboBoxOre()
        {
            var ore = new List<string>();
            for (int i = 0; i < 24; i++)
            {
                ore.Add($"{i:D2}:00");
            }

            cmbLvDeLa.ItemsSource = ore;
            cmbLvPanaLa.ItemsSource = ore;
            cmbSdDeLa.ItemsSource = ore;
            cmbSdPanaLa.ItemsSource = ore;
        }

        private async Task IncarcaSiPopuleazaSetariSistemAsync()
        {
            try
            {
                using (var context = new GymContext())
                {
                    _setariCurente = await context.SetariSistem.FirstOrDefaultAsync();

                    if (_setariCurente == null)
                    {
                        _setariCurente = new SetariSistem
                        {
                            Adresa = "Str. Principală Nr. 45, Timișoara",
                            Program = "LV:07:00-23:00|SD:09:00-20:00",
                            TelefonSuport = "0722 123 456"
                        };
                        context.SetariSistem.Add(_setariCurente);
                        await context.SaveChangesAsync();
                    }
                }

                // Populare TextBox-uri din Setări
                
                txtAdresaSala.Text = _setariCurente.Adresa;
                txtTelefonSuport.Text = _setariCurente.TelefonSuport; // INTEGRAT

                // Populare elemente vizuale pe Dashboard
               
                lblDashAdresa.Text = _setariCurente.Adresa;
                lblDashTelefon.Text = string.IsNullOrWhiteSpace(_setariCurente.TelefonSuport) ? "Nespecificat" : _setariCurente.TelefonSuport; // INTEGRAT

                if (!string.IsNullOrEmpty(_setariCurente.Program) && _setariCurente.Program.Contains("|"))
                {
                    var parti = _setariCurente.Program.Split('|');
                    var parteLV = parti[0].Replace("LV:", "").Split('-');
                    var parteSD = parti[1].Replace("SD:", "").Split('-');

                    if (parteLV.Length == 2 && parteSD.Length == 2)
                    {
                        cmbLvDeLa.SelectedItem = parteLV[0];
                        cmbLvPanaLa.SelectedItem = parteLV[1];
                        cmbSdDeLa.SelectedItem = parteSD[0];
                        cmbSdPanaLa.SelectedItem = parteSD[1];

                        lblDashProgram.Text = $"Luni - Vineri: {parteLV[0]} - {parteLV[1]}\nSâmbătă - Duminică: {parteSD[0]} - {parteSD[1]}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea inițială a setărilor: {ex.Message}");
            }
        }

        private async void Setari_Click(object sender, RoutedEventArgs e)
        {
            panelDashboard.Visibility = Visibility.Collapsed;
            panelMembri.Visibility = Visibility.Collapsed;
            panelSetari.Visibility = Visibility.Visible;

            using (var context = new GymContext())
            {
                _setariCurente = await context.SetariSistem.FirstOrDefaultAsync();

                if (_setariCurente == null)
                {
                    _setariCurente = new SetariSistem
                    {
                        Adresa = "Str. Principală Nr. 45, Timișoara",
                        Program = "LV:07:00-23:00|SD:09:00-20:00",
                        TelefonSuport = "0722 123 456"
                    };
                    context.SetariSistem.Add(_setariCurente);
                    await context.SaveChangesAsync();
                }
            }

            txtAdresaSala.Text = _setariCurente.Adresa;
            txtTelefonSuport.Text = _setariCurente.TelefonSuport; // INTEGRAT

            try
            {
                var parti = _setariCurente.Program.Split('|');
                var parteLV = parti[0].Replace("LV:", "").Split('-');
                var parteSD = parti[1].Replace("SD:", "").Split('-');

                cmbLvDeLa.SelectedItem = parteLV[0];
                cmbLvPanaLa.SelectedItem = parteLV[1];
                cmbSdDeLa.SelectedItem = parteSD[0];
                cmbSdPanaLa.SelectedItem = parteSD[1];
            }
            catch
            {
                cmbLvDeLa.SelectedIndex = 7;
                cmbLvPanaLa.SelectedIndex = 23;
                cmbSdDeLa.SelectedIndex = 9;
                cmbSdPanaLa.SelectedIndex = 20;
            }
        }


        private async void SalveazaSetari_Click(object sender, RoutedEventArgs e)
        {
            if ( string.IsNullOrWhiteSpace(txtAdresaSala.Text) || string.IsNullOrWhiteSpace(txtTelefonSuport.Text))
            {
                MessageBox.Show("Adresa și telefonul sunt obligatorii!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbLvDeLa.SelectedItem == null || cmbLvPanaLa.SelectedItem == null ||
                cmbSdDeLa.SelectedItem == null || cmbSdPanaLa.SelectedItem == null)
            {
                MessageBox.Show("Te rog selectează toate intervalele orare pentru program!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string lvDeLa = cmbLvDeLa.SelectedItem.ToString();
            string lvPanaLa = cmbLvPanaLa.SelectedItem.ToString();
            string sdDeLa = cmbSdDeLa.SelectedItem.ToString();
            string sdPanaLa = cmbSdPanaLa.SelectedItem.ToString();

            string programPentruDB = $"LV:{lvDeLa}-{lvPanaLa}|SD:{sdDeLa}-{sdPanaLa}";
            string programAfisajDashboard = $"Luni - Vineri: {lvDeLa} - {lvPanaLa}\nSâmbătă - Duminică: {sdDeLa} - {sdPanaLa}";

            try
            {
                using (var context = new GymContext())
                {
                    var setariDb = await context.SetariSistem.FirstOrDefaultAsync(s => s.Id == _setariCurente.Id);

                    if (setariDb != null)
                    {
                       
                        setariDb.Adresa = txtAdresaSala.Text.Trim();
                        setariDb.TelefonSuport = txtTelefonSuport.Text.Trim(); // SALVARE ÎN DB INTEGRATĂ
                        setariDb.Program = programPentruDB;

                        await context.SaveChangesAsync();
                        _setariCurente = setariDb;

                        
                        lblDashAdresa.Text = _setariCurente.Adresa;
                        lblDashTelefon.Text = _setariCurente.TelefonSuport; // UPDATE LIVE PE DASHBOARD
                        lblDashProgram.Text = programAfisajDashboard;

                        MessageBox.Show("Setările au fost salvate cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvare: {ex.Message}", "Eroare Critică", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AdaugaAbonament_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aici vom adăuga un tip nou de abonament!");
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            panelDashboard.Visibility = Visibility.Visible;
            panelMembri.Visibility = Visibility.Collapsed;
            panelSetari.Visibility = Visibility.Collapsed;
        }

        private async void Membri_Click(object sender, RoutedEventArgs e)
        {
            panelDashboard.Visibility = Visibility.Collapsed;
            panelMembri.Visibility = Visibility.Visible;
            panelSetari.Visibility = Visibility.Collapsed;
            await IncarcaMembri();
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

        private async void dgUtilizatori_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is Utilizator utilizatorModificat)
            {
                if (!string.IsNullOrEmpty(utilizatorModificat[nameof(Utilizator.Username)]) ||
                    !string.IsNullOrEmpty(utilizatorModificat[nameof(Utilizator.Email)]) ||
                    !string.IsNullOrEmpty(utilizatorModificat[nameof(Utilizator.Parola)]) ||
                    !string.IsNullOrEmpty(utilizatorModificat[nameof(Utilizator.Telefon)]))
                {
                    MessageBox.Show("Modificarea a fost respinsă! Celulele marcate cu roșu conțin date invalide.",
                                    "Eroare Validare Tabel", MessageBoxButton.OK, MessageBoxImage.Error);

                    e.Cancel = true;
                    await IncarcaMembri();
                    return;
                }

                await Dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        await _utilizatorRepository.UpdateAsync(utilizatorModificat);
                        var index = _toțiUtilizatorii.FindIndex(u => u.Id == utilizatorModificat.Id);
                        if (index != -1) _toțiUtilizatorii[index] = utilizatorModificat;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Eroare la salvare: {ex.Message}");
                        await IncarcaMembri();
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private async void AdaugaUtilizator_Click(object sender, RoutedEventArgs e)
        {
            AdaugaUtilizatorWindow dialog = new AdaugaUtilizatorWindow();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                var nouUtilizator = dialog.UtilizatorCreat;

                try
                {
                    await _utilizatorRepository.AddAsync(nouUtilizator);
                    _toțiUtilizatorii.Add(nouUtilizator);

                    dgUtilizatori.ItemsSource = null;
                    dgUtilizatori.ItemsSource = _toțiUtilizatorii;

                    MessageBox.Show($"Utilizatorul {nouUtilizator.Username} a fost creat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la salvarea în baza de date: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}
*/