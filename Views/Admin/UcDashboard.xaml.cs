using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GymFitnessSystem.Data;
using GymFitnessSystem.Models;

namespace GymFitnessSystem.Views.Admin
{
    public partial class UcDashboard : UserControl
    {
        public UcDashboard()
        {
            InitializeComponent();

            // Încărcăm asincron toate datele din SQL
            _ = IncarcaDateDashboardAsync();
        }

        private async Task IncarcaDateDashboardAsync()
        {
            try
            {
                using (var context = new GymContext())
                {
                    // Incarcare setari sistem (adresa, program, telefon suport)
                    var setari = await context.SetariSistem.FirstOrDefaultAsync();
                    if (setari != null)
                    {
                        lblDashAdresa.Text = setari.Adresa;
                        lblDashTelefon.Text = string.IsNullOrWhiteSpace(setari.TelefonSuport) ? "Nespecificat" : setari.TelefonSuport;

                        if (!string.IsNullOrEmpty(setari.Program) && setari.Program.Contains("|"))
                        {
                            var parti = setari.Program.Split('|');
                            var parteLV = parti[0].Replace("LV:", "").Split('-');
                            var parteSD = parti[1].Replace("SD:", "").Split('-');

                            if (parteLV.Length == 2 && parteSD.Length == 2)
                            {
                                lblDashProgram.Text = $"Luni - Vineri: {parteLV[0]} - {parteLV[1]}\nSâmbătă - Duminică: {parteSD[0]} - {parteSD[1]}";
                            }
                        }
                    }

                    //Graficcul de utilizatori activi pe roluri (Admin, Angajat, Client)

                    // [Codul de extragere date]
                    var roluriBrute = await context.Utilizatori
                        .Where(u => u.EsteActiv == true)
                        .Select(u => (int)u.Rol)
                        .ToListAsync();

                    double countAdmin = roluriBrute.Count(r => r == 0);
                    double countAngajat = roluriBrute.Count(r => r == 1);
                    double countClient = roluriBrute.Count(r => r == 2);

                    //  Actualizăm SERIILE (Barele din grafic)
                    if (ChartAbonamente.Series != null && ChartAbonamente.Series.Any())
                    {
                        // Dacă seria există deja, doar îi schimbăm valorile din spate
                        var serieExistenta = ChartAbonamente.Series.First() as ColumnSeries<double>;
                        if (serieExistenta != null)
                        {
                            serieExistenta.Values = new double[] { countAdmin, countAngajat, countClient };
                        }
                    }
                    else
                    {
                        // Dacă e prima rulare și e complet nul, îl inițializăm o singură dată
                        ChartAbonamente.Series = new ISeries[]
                        {
                            new ColumnSeries<double>
                            {
                                Values = new double[] { countAdmin, countAngajat, countClient },
                                Name = "Conturi Active",
                                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
                            }
                        };
                    }

                    //  Actualizăm AXA X (Etichetele de jos)
                    if (ChartAbonamente.XAxes != null && ChartAbonamente.XAxes.Any())
                    {
                        var axaExistenta = ChartAbonamente.XAxes.First() as Axis;
                        if (axaExistenta != null)
                        {
                            axaExistenta.Labels = new string[] { "Admini", "Angajați", "Clienți" };
                        }
                    }
                    else
                    {
                        // Dacă axa e nulă, o setăm acum
                        ChartAbonamente.XAxes = new Axis[]
                        {
                            new Axis { Labels = new string[] { "Admini", "Angajați", "Clienți" } }
                        };
                    }

                    //Grafic cu metodele de plata preferate (Cash vs Card)
                    // Filtrăm plățile direct după tipul Enum StatusPlata.Platit
                    var toatePlatile = await context.Plati.ToListAsync();

                    // Numărăm absolut toate înregistrările venite de la clienți
                    int totalCash = toatePlatile.Count(p => p.Metoda == MetodaPlata.Cash);
                    int totalCard = toatePlatile.Count(p => p.Metoda == MetodaPlata.Card);

                    // trimitem valorile direct ca double (recomandat de LiveCharts)
                    chartMetodePlata.Series = new ISeries[]
                    {
                        new PieSeries<double> { Values = new double[] { totalCash }, Name = "Cash" },
                        new PieSeries<double> { Values = new double[] { totalCard }, Name = "Card" }
                    };


                    // DETECTARE ACTIVITATE RECENTĂ DINAMICĂ (NUME & SUME REALE) 
                    var activitatiFeed = new List<string>();

                    // Luăm ultimii 2 membri adăugați și le afișăm stringul cu NumeComplet
                    var ultimiiMembri = await context.Utilizatori.OrderByDescending(u => u.Id).Take(2).ToListAsync();
                    foreach (var m in ultimiiMembri)
                    {
                        string numeAfisat = string.IsNullOrWhiteSpace(m.NumeComplet) ? m.Username : m.NumeComplet;
                        activitatiFeed.Add($"• Membru nou înregistrat: {numeAfisat} (Cont: {m.Rol})");
                    }

                    // Luăm ultimele 2 plăți salvate și extragem proprietatea .Suma
                    var ultimelePlati = await context.Plati.OrderByDescending(p => p.Id).Take(2).ToListAsync();
                    foreach (var p in ultimelePlati)
                    {
                        activitatiFeed.Add($"• Încasare confirmată: +{p.Suma} RON primită prin metoda [{p.Metoda}]");
                    }

                    // Fail-safe dacă nu există date deloc
                    if (activitatiFeed.Count == 0)
                    {
                        activitatiFeed.Add("• Sistemul de monitorizare este activ. Niciun log generat încă.");
                    }

                    // Împingem datele curate în ListBox-ul din XAML
                    lstActivitateRecenta.ItemsSource = activitatiFeed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea datelor pe Dashboard: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}