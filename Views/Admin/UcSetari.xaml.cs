using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using GymFitnessSystem.Data;
using GymFitnessSystem.Models;

namespace GymFitnessSystem.Views.Admin
{
    public partial class UcSetari : UserControl
    {
        private SetariSistem _setariCurente;

        public UcSetari ()
        {
            InitializeComponent();
            InitializeazaComboBoxOre();
            _ = IncarcaSiPopuleazaSetariSistemAsync();
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

                txtAdresaSala.Text = _setariCurente.Adresa;
                txtTelefonSuport.Text = _setariCurente.TelefonSuport;

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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea setărilor: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SalveazaSetari_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAdresaSala.Text) || string.IsNullOrWhiteSpace(txtTelefonSuport.Text))
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

            try
            {
                using (var context = new GymContext())
                {
                    var setariDb = await context.SetariSistem.FirstOrDefaultAsync(s => s.Id == _setariCurente.Id);

                    if (setariDb != null)
                    {
                        setariDb.Adresa = txtAdresaSala.Text.Trim();
                        setariDb.TelefonSuport = txtTelefonSuport.Text.Trim();
                        setariDb.Program = programPentruDB;

                        await context.SaveChangesAsync();
                        _setariCurente = setariDb;

                        MessageBox.Show("Setările au fost salvate cu succes!\n(Modificările vor fi vizibile pe Dashboard la următoarea accesare)", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvare: {ex.Message}", "Eroare Critică", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}