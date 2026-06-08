using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Models.Produs_Serviciu; 
using GymFitnessSystem.Services;
using GymFitnessSystem.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GymFitnessSystem.Views.Admin
{
    public partial class UcManagement : UserControl
    {
        public ISeries[] GraficSerii { get; set; }
        public Axis[] GraficAxaX { get; set; }
        public Axis[] GraficAxaY { get; set; }
        public Array MetodePlataLista => Enum.GetValues(typeof(MetodaPlata));
        public Array StatusuriLista => Enum.GetValues(typeof(StatusPlata));
        
        private ObservableCollection<double> _valoriGrafic = new ObservableCollection<double>(new double[12]);


        public UcManagement()
        {
            InitializeComponent();
            this.DataContext = this; 
            IncarcaDatele();         
        } 
           
       
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void IncarcaDatele()
        {
            try
            {
                // Setăm selecția pe "Toate Categoriile" ceea ce va apela automat evenimentele de SelectionChanged scrise mai sus
                if (cmbFiltruServicii != null) cmbFiltruServicii.SelectedIndex = 0;
                if (cmbFiltruProduse != null) cmbFiltruProduse.SelectedIndex = 0;
                IncarcaRapoarteFinanciare();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea datelor: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Eveniment apelat automat când schimbi perioada în ComboBox sau modifici DatePicker-ele
        private void Filtre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Verificăm dacă elementele din UI sunt inițializate complet pentru a evita crash la pornire
            if (cbPerioada == null || panelDateCustom == null) return;

            // Afișăm sau ascundem DatePicker-ele în funcție de selecție
            if (cbPerioada.SelectedIndex == 4) // "Interval Personalizat"
            {
                panelDateCustom.Visibility = Visibility.Visible;
            }
            else
            {
                panelDateCustom.Visibility = Visibility.Collapsed;
            }

            // De fiecare dată când se schimbă ceva, reîncărcăm și filtrăm datele din DB
            IncarcaRapoarteFinanciare();
        }



        private void ActualizeazaGraficAnual(List<Plata> toatePlatile)
        {
            try
            {
                //  Inițializăm vectorul de 12 poziții cu 0
                double[] sumeLuni = new double[12];
                int anulCurent = DateTime.Now.Year;

                //  Filtrăm și grupăm sumele din baza de date
                if (toatePlatile != null)
                {
                    foreach (var plata in toatePlatile)
                    {
                        // Verificare curată pentru structura ta de date
                        if (plata.DataPlatii != null && plata.DataPlatii.Year == anulCurent)
                        {
                            //  Convertim direct la string fără '?.' deoarece enum-ul are valoare implicită
                            string statusStr = plata.Status.ToString();

                            if (statusStr == "Platit" || statusStr == "Finalizata")
                            {
                                int lunaIndex = plata.DataPlatii.Month - 1;
                                if (lunaIndex >= 0 && lunaIndex < 12)
                                {
                                    sumeLuni[lunaIndex] += (double)plata.Suma;
                                }
                            }
                        }
                    }
                }

                //  Setările Axie X și Axei Y aplicate la fiecare împrospătare
                var eticheteLuni = new string[] { "Ian", "Feb", "Mar", "Apr", "Mai", "Iun", "Iul", "Aug", "Sep", "Oct", "Noi", "Dec" };

                GraficAxaX = new Axis[]
                {
                    new Axis
                    {
                        Labels = eticheteLuni,
                        LabelsRotation = 0,
                        MinStep = 1,
                        ForceStepToMin = true,
                        MinLimit = -0.5,
                        MaxLimit = 11.5
                    }
                };

                GraficAxaY = new Axis[]
                {
                    new Axis
                    {
                        Labeler = value => $"{value:N0} RON",
                        MinLimit = -600,         // Forțează începutul de la -600 barat
                        MinStep = 500,        // Distribuție aerisită la fiecare 500 RON
                        ForceStepToMin = true,
                        MaxLimit = null
                    }
                };

                //  Legăm sau actualizăm seria de date
                if (GraficSerii == null)
                {
                    GraficSerii = new ISeries[]
                    {
                new ColumnSeries<double>
                {
                    Name = "Încasări",
                    Values = _valoriGrafic,
                    Stroke = null,
                    MaxBarWidth = 35,
                    Padding = 5
                }
                    };
                }

                //  Transferăm valorile calculate în colecția graficului
                for (int i = 0; i < 12; i++)
                {
                    if (_valoriGrafic.Count > i)
                    {
                        _valoriGrafic[i] = sumeLuni[i];
                    }
                    else
                    {
                        _valoriGrafic.Add(sumeLuni[i]);
                    }
                }
  }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la randare grafic: {ex.Message}");
            }
        }
        private void IncarcaRapoarteFinanciare()
        {
            try
            {
                using (var dbContext = new GymContext())
                {
                    
                    //  Includem atât Activitatea, cât și Oferta aferentă
                    var plati = dbContext.Plati
                                  .Include(p => p.Activitate)
                                  .ThenInclude(a => a.Oferta) // Aici accesăm obiectul care are numele
                                  .ToList();


                    //  Filtrare Perioadă
                    var platiFiltrate = plati.AsQueryable();

                    foreach (var p in plati)
                    {
                        // Accesăm Numele prin relația: Plata -> Activitate -> Oferta -> Nume
                        
                        p.NumeArticol = p.Activitate?.Oferta?.Nume ?? "Serviciu/Abonament";
                    }

                    dgVanzariRapoarte.ItemsSource = plati;
                    if (cbPerioada?.SelectedIndex > 0)
                    {
                        DateTime acum = DateTime.Now;
                        switch (cbPerioada.SelectedIndex)
                        {
                            case 1: // Săptămâna
                                int zilePanaLuni = (int)acum.DayOfWeek - (int)DayOfWeek.Monday;
                                if (zilePanaLuni < 0) zilePanaLuni += 7;
                                DateTime inceputSapt = acum.Date.AddDays(-zilePanaLuni);
                                platiFiltrate = platiFiltrate.Where(p => p.DataPlatii >= inceputSapt);
                                break;
                            case 2: // Luna
                                platiFiltrate = platiFiltrate.Where(p => p.DataPlatii >= new DateTime(acum.Year, acum.Month, 1));
                                break;
                            case 3: // Anul
                                platiFiltrate = platiFiltrate.Where(p => p.DataPlatii >= new DateTime(acum.Year, 1, 1));
                                break;
                            case 4: // Personalizat
                                if (dpDeLa.SelectedDate.HasValue) platiFiltrate = platiFiltrate.Where(p => p.DataPlatii >= dpDeLa.SelectedDate.Value.Date);
                                if (dpPanaLa.SelectedDate.HasValue) platiFiltrate = platiFiltrate.Where(p => p.DataPlatii <= dpPanaLa.SelectedDate.Value.Date.AddDays(1));
                                break;
                        }
                    }

                    // Filtrare Sursă
                    if (cbSursaVenit?.SelectedIndex > 0)
                    {
                        // index pe ActivitateId în baza de date pentru performanță
                        var idComenzi = dbContext.Comenzi.Select(c => c.Id).ToList();
                        var idProgramari = dbContext.Programari.Select(p => p.Id).ToList();

                        if (cbSursaVenit.SelectedIndex == 1) platiFiltrate = platiFiltrate.Where(p => idProgramari.Contains(p.ActivitateId));
                        else if (cbSursaVenit.SelectedIndex == 2) platiFiltrate = platiFiltrate.Where(p => idComenzi.Contains(p.ActivitateId));
                    }

                    var listaFinala = platiFiltrate.OrderByDescending(p => p.DataPlatii).ToList();

                    //  Afișare
                    dgVanzariRapoarte.ItemsSource = listaFinala;

                    //  Statistici (Doar pe statusul 'Platit')
                    decimal totalIncasari = listaFinala.Where(p => p.Status == StatusPlata.Platit).Sum(p => p.Suma);
                    txtTotalIncasari.Text = $"{totalIncasari:F2} RON";

                    txtNumarPlati.Text = listaFinala.Count(p => p.Status == StatusPlata.Platit).ToString();
                    txtNumarInProcesare.Text = listaFinala.Count(p => p.Status == StatusPlata.InAsteptare).ToString();
                    txtNumarAnulate.Text = listaFinala.Count(p => p.Status == StatusPlata.Anulat).ToString();

                    ActualizeazaGraficAnual(listaFinala);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea raportului: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //  METODA PENTRU SERVICII

        private void dgServicii_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var serviciuEditat = e.Row.Item as Serviciu;
                if (serviciuEditat == null) return;

                string erori = "";
                if (string.IsNullOrWhiteSpace(serviciuEditat.Nume)) erori += "• Numele nu poate fi gol.\n";
                if (serviciuEditat.Pret < 0) erori += "• Prețul nu poate fi negativ.\n";
                if (serviciuEditat.DurataZile <= 0) erori += "• Durata trebuie să fie de minimum 1 zi.\n";

                if (!string.IsNullOrEmpty(erori))
                {
                    MessageBox.Show(erori, "Eroare Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Cancel = true;
                    return;
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        using (var dbContext = new GymContext())
                        {
                            dbContext.Entry(serviciuEditat).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            dbContext.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Eroare la salvarea serviciului: {ex.Message}", "Eroare SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        //  METODA PENTRU PRODUSE (+ verificarea stocului și NOTIFICAREA)

        private void dgProduse_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var produsEditat = e.Row.Item as Produs;
                if (produsEditat == null) return;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        using (var dbContext = new GymContext())
                        {
                            dbContext.Entry(produsEditat).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                            // Verificăm dacă stocul a devenit 0 și generăm notificarea utilizând corect contextul 'dbContext'
                            if (produsEditat.Stoc == 0)
                            {
                                var alertaStoc = new Notificare
                                {
                                    UtilizatorId = 1, // ID-ul adminului implicit
                                    Titlu = "🚨 STOC EPUIZAT",
                                    Mesaj = $"Produsul '{produsEditat.Nume}' a rămas fără stoc la bar!",
                                    DataTrimitere = DateTime.Now,
                                    EsteCitit = false
                                };

                                dbContext.Notificari.Add(alertaStoc);
                            }

                            dbContext.SaveChanges();
                        }

                        // Reîncărcăm rapoartele ca să actualizăm interfața live
                        IncarcaRapoarteFinanciare();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Eroare la salvarea produsului: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        //Bara de filtrare de deasupra tabelelor
        //  BARA DE FILTRARE DE DEASUPRA TABELELOR (SERVICII)
        private void CmbFiltruServicii_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgServicii == null || cmbFiltruServicii == null) return;
            string filtru = (cmbFiltruServicii.SelectedItem as ComboBoxItem)?.Content.ToString();

            try
            {
                using (var context = new GymContext())
                {
                    var toateServiciile = context.Servicii.ToList();
                    if (filtru == "Toate Categoriile" || string.IsNullOrEmpty(filtru))
                    {
                        dgServicii.ItemsSource = toateServiciile;
                    }
                    else
                    {
                        // Curățăm emoji-ul din filtru pentru a compara doar textul curat
                        string filtruCurat = string.Concat(filtru.Where(c => !char.IsSymbol(c) && c < 128)).Trim();

                        dgServicii.ItemsSource = toateServiciile.Where(s => s.Descriere != null &&
                            (s.Descriere.Contains(filtru) || s.Descriere.Contains(filtruCurat))).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la filtrare servicii: {ex.Message}");
            }
        }

        //  BARA DE FILTRARE DE DEASUPRA TABELELOR (PRODUSE)
        private void CmbFiltruProduse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProduse == null || cmbFiltruProduse == null) return;
            string filtru = (cmbFiltruProduse.SelectedItem as ComboBoxItem)?.Content.ToString();

            try
            {
                using (var context = new GymContext())
                {
                    var toateProdusele = context.Produse.ToList();
                    if (filtru == "Toate Categoriile" || string.IsNullOrEmpty(filtru))
                    {
                        dgProduse.ItemsSource = toateProdusele;
                    }
                    else
                    {
                        // Iei textul direct din ComboBox (care acum conține pastila și textul corect datorită modificării din XAML)
                        string filtruCurat = filtru;

                        // Filtrarea simplă care va căuta direct textul exact în SQL
                        dgProduse.ItemsSource = toateProdusele
                            .Where(p => p.Descriere != null && p.Descriere.Contains(filtruCurat))
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la filtrare produse: {ex.Message}");
            }
        }

        // Metodele asociate butoanelor din XAML pt adaugare de Seviciu si de Produs

        private void AdaugaServiciu_Click(object sender, RoutedEventArgs e)
        {
            AdaugaServiciuWindow dialog = new AdaugaServiciuWindow();
            dialog.Owner = Window.GetWindow(this); // Să apară centrată peste aplicație

            if (dialog.ShowDialog() == true)
            {
                var nouServiciu = dialog.ServiciuCreat;
                if (nouServiciu == null) return;

                try
                {
                    using (var context = new GymContext())
                    {
                        context.Servicii.Add(nouServiciu);
                        context.SaveChanges();
                    }

                    IncarcaDatele(); // Dăm refresh la DataGrid-uri ca să apară noul serviciu pe ecran
                    MessageBox.Show($"Abonamentul '{nouServiciu.Nume}' a fost adăugat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la salvarea în baza de date: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void AdaugaProdus_Click(object sender, RoutedEventArgs e)
        {
            AdaugaProdusWindow dialog = new AdaugaProdusWindow();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                var nouProdus = dialog.ProdusCreat;
                if (nouProdus == null) return;

                try
                {
                    using (var context = new GymContext())
                    {
                        context.Produse.Add(nouProdus);
                        context.SaveChanges();
                    }

                    IncarcaDatele(); // Refresh la tabele
                    MessageBox.Show($"Produsul '{nouProdus.Nume}' a fost adăugat în stocul barului!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la salvarea în baza de date: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

       
       
        private void AprovizioneazaRapid_Click(object sender, RoutedEventArgs e)
        {
            if (dgProduse.SelectedItem is Produs p)
            {
                try
                {
                    using (var context = new GymContext())
                    {
                        p.Stoc += 20; // Adăugăm 20 de bucăți direct pe obiectul selectat
                        context.Produse.Update(p);
                        context.SaveChanges();
                    }
                    IncarcaDatele(); // Dăm refresh la tabele ca să vedem noul stoc instant
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la aprovizionare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Te rog selectează un produs din tabel mai întâi!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dgVanzariRapoarte_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //  Verificăm dacă editarea s-a finalizat cu succes
            if (e.EditAction == DataGridEditAction.Commit)
            {
                //  Obținem obiectul editat (din rândul curent)
                var plataEditata = e.Row.Item as Plata;

                if (plataEditata != null)
                {
                    using (var db = new GymContext())
                    {
                        // 3. Attach obiectului pentru a-l marca ca fiind modificat
                        db.Plati.Attach(plataEditata);
                        db.Entry(plataEditata).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                        // 4. Salvează în SQL
                        db.SaveChanges();
                    }
                }
            }
        }

        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            // Înregistrare licență moca Community
            QuestPDF.Settings.License = LicenseType.Community;

            var listaPlatiCurente = dgVanzariRapoarte.ItemsSource as List<Plata>;

            if (listaPlatiCurente == null || listaPlatiCurente.Count == 0)
            {
                MessageBox.Show("Nu există date de exportat în acest moment pe baza filtrelor selectate!", "Export Anulat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog dialogSalvare = new SaveFileDialog
            {
                Filter = "Fișier PDF (*.pdf)|*.pdf",
                FileName = $"Raport_Financiar_{DateTime.Now:yyyy_MM_dd}",
                Title = "Salvează Raportul Contabil în format PDF"
            };

            if (dialogSalvare.ShowDialog() == true)
            {
                try
                {
                    decimal totalIncasat = listaPlatiCurente.Where(p => p.Status == StatusPlata.Platit).Sum(p => p.Suma);
                    int nrPlati = listaPlatiCurente.Count(p => p.Status == StatusPlata.Platit);

                    string perioadaText = cbPerioada?.SelectedItem is ComboBoxItem cbI1 ? cbI1.Content.ToString() : "Toată Perioada";
                    string sursaText = cbSursaVenit?.SelectedItem is ComboBoxItem cbI2 ? cbI2.Content.ToString() : "Toate Sursele";

                    //  Generarea efectivă a PDF-ului pe disc
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(40);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                            // ANTET (HEADER)
                            page.Header().Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("FitZone 1 ~ Gym Management").FontSize(20).Bold();
                                    column.Item().Text($"Generat la data: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10);
                                    column.Item().Text($"Tip Perioadă: {perioadaText} | Sursă: {sursaText}").FontSize(10).Italic();
                                });
                            });

                            // CONȚINUT (CONTENT)
                            page.Content().PaddingVertical(20).Column(column =>
                            {
                                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(10).Row(r =>
                                {
                                    r.RelativeItem().Text($"Total Încasat (Tranzacții Finalizate): {totalIncasat:F2} RON").Bold();
                                    r.RelativeItem().Text($"Număr Tranzacții Active: {nrPlati}").Bold();
                                });

                                column.Item().PaddingTop(15).Text("Registru Detaliat Plăți:").FontSize(14).Bold();

                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.3f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("ID").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Data Plății").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Metodă").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Status").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Suma").Bold().FontColor(Colors.White);
                                    });

                                    foreach (var plata in listaPlatiCurente)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(plata.Id.ToString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(plata.DataPlatii.ToString("dd.MM.yyyy HH:mm"));
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(plata.Metoda.ToString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(plata.Status.ToString()).Bold();
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{plata.Suma:F2} RON");
                                    }
                                });
                            });

                            // SUBSOL (FOOTER)
                            page.Footer().AlignRight().Text(x =>
                            {
                                x.Span("Pagina ");
                                x.CurrentPageNumber();
                                x.Span(" din ");
                                x.TotalPages();
                            });
                        });
                    }).GeneratePdf(dialogSalvare.FileName);

                   
                    // SALVAREA ÎN BAZA DE DATE
                   
                    using (var dbContext = new GymContext())
                    {
                        var istoricRaport = new Raport
                        {
                            Tip = TipRaport.Vanzari,
                            DataGenerare = DateTime.Now,
                            GeneratDeUtilizatorId = UserSession.LoggedInUserId,
                            DetaliiSauPath = dialogSalvare.FileName, // Salvăm locația unde a fost generat PDF-ul
                            PerioadaVizat = perioadaText // Ex: "Luna", "Săptămâna", "Interval Personalizat"
                        };

                        dbContext.Rapoarte.Add(istoricRaport);
                        dbContext.SaveChanges();
                    }
                    // ==========================================

                    MessageBox.Show($"Raportul financiar a fost exportat cu succes și salvat în istoric:\n{dialogSalvare.FileName}", "Export PDF Reușit", MessageBoxButton.OK, MessageBoxImage.Information);

                    // IncarcaIstoricRapoarte(); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"A apărut o eroare la generarea fișierului PDF sau salvarea în istoric: {ex.Message}", "Eroare Export", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}