using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using GymFitnessSystem.Data;
using System.Linq;

namespace GymFitnessSystem.Views.Admin
{
    public partial class UcPerformanta : UserControl
    {
        public UcPerformanta()
        {
            InitializeComponent();
            _ = MasoaraPerformanta();
        }

        private async void BtnDiagnostic_Click(object sender, RoutedEventArgs e)
        {
            AdaugaLog("Se inițializează diagnosticarea manuală la cererea administratorului...");
            await MasoaraPerformanta();
        }

        private void AdaugaLog(string mesaj)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            lstLoguri.Items.Insert(0, $"[{timestamp}] - {mesaj}"); // Pune cele mai noi loguri sus
        }

        private async Task MasoaraPerformanta()
        {
            try
            {
                // ---------------- METRICA 1: RAM FIZIC REAL (Working Set) ----------------
                Process procesCurent = Process.GetCurrentProcess();
                // WorkingSet64 măsoară memoria RAM fizică pe care sistemul de operare o alocă acum aplicației
                long memorieBytes = procesCurent.WorkingSet64;
                double memorieMegaBytes = memorieBytes / (1024.0 * 1024.0);

                lblRam.Text = $"{memorieMegaBytes:F2} MB";
                prgRam.Value = memorieMegaBytes;
                AdaugaLog($"RAM fizic utilizat de aplicație: {memorieMegaBytes:F2} MB.");

                // ---------------- METRICA 2: LATENȚĂ REALĂ SQL (Fără Cache EF) ----------------
                Stopwatch swSql = Stopwatch.StartNew();
                int totalUtilizatori = 0;

                using (var context = new GymContext())
                {
                    // 1. Adăugăm .AsNoTracking() ca să forțăm EF Core să interogheze baza de date direct, fără să ia datele din memoria cache locală
                    // 2. Facem o operație mai complexă decât un simplu Count (ex: sortare și filtrare pe ultimele înregistrări)
                    var ultimelePlati = await context.Plati
                        .AsNoTracking()
                        .OrderByDescending(p => p.Id)
                        .Take(20)
                        .ToListAsync();

                    totalUtilizatori = await context.Utilizatori.AsNoTracking().CountAsync();
                }
                swSql.Stop();
                long timpSqlMs = swSql.ElapsedMilliseconds;
                lblSqlTime.Text = $"{timpSqlMs} ms";

                if (timpSqlMs < 15)
                {
                    lblSqlStatus.Text = "Status: Conexiune excelentă";
                    lblSqlStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (timpSqlMs < 80)
                {
                    lblSqlStatus.Text = "Status: Conexiune normală";
                    lblSqlStatus.Foreground = System.Windows.Media.Brushes.Orange;
                }
                else
                {
                    lblSqlStatus.Text = "Status: Latență mare detectată!";
                    lblSqlStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
                AdaugaLog($"Test interogare SQL reală finalizat în {timpSqlMs} ms.");

                // ---------------- METRICA 3: VOLUMUL TOTAL DE DATE (PRAGURI REALE) ----------------
                int totalInregistrariLogistica = 0;
                using (var context = new GymContext())
                {
                    int plati = await context.Plati.AsNoTracking().CountAsync();
                    totalInregistrariLogistica = totalUtilizatori + plati;
                }
                lblVolumeDate.Text = $"{totalInregistrariLogistica} rânduri";

                // Schimbăm textul fals cu unul tehnic corect. 10.000 e infim, dar punem un prag mai realist (ex: 50.000)
                if (totalInregistrariLogistica < 50000)
                {
                    lblVolumeStatus.Text = "Status: Volum redus (Performanță nativă maximă)";
                    lblVolumeStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    lblVolumeStatus.Text = "Status: Volum moderat (Se recomandă verificare indecși)";
                    lblVolumeStatus.Foreground = System.Windows.Media.Brushes.Orange;
                }
                AdaugaLog($"Volum total stocat în tabelele analizate: {totalInregistrariLogistica} rânduri.");

                // ---------------- METRICA 4: LATENȚĂ REALĂ UI (Dispatcher Starvation) ----------------
                Stopwatch swUi = Stopwatch.StartNew();

                // TRICUL: Trimitem o acțiune goală în coada UI-ului cu prioritate Background. 
                // Dacă thread-ul principal este ocupat cu randări grele sau e blocat de procese masive,
                // acțiunea noastră va fi pusă la coadă și va aștepta. Stopwatch-ul măsoară exact acest timp de așteptare (LAG).
                await Application.Current.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Background);

                swUi.Stop();
                double timpUiMs = swUi.Elapsed.TotalMilliseconds;
                lblUiTime.Text = $"{timpUiMs:F2} ms";

                if (timpUiMs < 5)
                {
                    lblUiStatus.Text = "Stare interfață: Foarte fluidă";
                    lblUiStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (timpUiMs < 25)
                {
                    lblUiStatus.Text = "Stare interfață: Micro-stutter / Încărcare sesizabilă";
                    lblUiStatus.Foreground = System.Windows.Media.Brushes.Orange;
                }
                else
                {
                    lblUiStatus.Text = "Stare interfață: Frame-drop / UI Blocat";
                    lblUiStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
                AdaugaLog($"Timp de răspuns al cozii UI (Dispatcher): {timpUiMs:F2} ms.");
                AdaugaLog("--- Diagnosticare reală realizată cu succes ---");
            }
            catch (Exception ex)
            {
                AdaugaLog($"[EROARE CRITICĂ] {ex.Message}");
            }
        }
    }
}