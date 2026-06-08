using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Models.Produs_Serviciu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GymFitnessSystem.Services;


namespace GymFitnessSystem.Views.Client
{
    public partial class UcShop : UserControl
    {
        public class ElementMagazin
        {
            public int IdOferta { get; set; }
            public bool EsteServiciu { get; set; }
            public string DenumireAfisare { get; set; }
            public string DescriereAfisare { get; set; }
            public decimal PretAfisare { get; set; }
            public string CategorieAfisare { get; set; }
            public string StocAfisare { get; set; }
            public Brush CuloareStoc { get; set; }
            public bool EsteDisponibil { get; set; }
            public object ObiectOriginal { get; set; }
        }

        private bool _initializat = false;
        private bool _seIncarcaFiltrele = false; // Flag crucial pentru a opri MessageBox-urile inutile

        public UcShop()
        {
            InitializeComponent();
            _initializat = true;
            IncarcaFiltreCategorii();
            IncarcaDateMagazin();
        }

        private void IncarcaFiltreCategorii()
        {
            if (!_initializat) return;

            _seIncarcaFiltrele = true;

            cmbCategorie.Items.Clear();
            cmbCategorie.Items.Add("🔍 Toate Categoriile");

            if (cmbTipOferta.SelectedIndex == 0) // Servicii
            {
                cmbCategorie.Items.Add("💪 Abonament");
                cmbCategorie.Items.Add("👥 Clasa de Grup");
                cmbCategorie.Items.Add("👟 Antrenor Personal");
            }
            else // Produse
            {
                cmbCategorie.Items.Add("🎒 Echipament și Accesorii");
                cmbCategorie.Items.Add("☕ Băuturi și Rehidratare");
                cmbCategorie.Items.Add("🍫 Gustări și Alimente Proteice");
                cmbCategorie.Items.Add("💊 Suplimente Mari și Vitamine"); // Aici poți pune cu diacritice!
            }

            cmbCategorie.SelectedIndex = 0;
            _seIncarcaFiltrele = false;
        }
        private void IncarcaDateMagazin()
        {
            // Dacă controlul nu e gata sau suntem în mijlocul populării categoriilor, ieșim fără să interogăm DB-ul
            if (!_initializat || _seIncarcaFiltrele) return;

            try
            {
                using (var context = new GymContext())
                {
                    List<ElementMagazin> itemiAfisare = new List<ElementMagazin>();
                    string categorieSelectata = cmbCategorie.SelectedItem?.ToString();

                    if (cmbTipOferta.SelectedIndex == 0) // SERVICII
                    {
                        var serviciiDinDb = context.Servicii.ToList();

                        foreach (var s in serviciiDinDb)
                        {
                            string descriereCompleta = s.Descriere ?? "";
                            string categorieExtrasa = "Serviciu";
                            string descriereCurata = "";

                            if (descriereCompleta.Contains("|"))
                            {
                                var bucati = descriereCompleta.Split('|');
                                categorieExtrasa = bucati[0].Trim();
                                descriereCurata = bucati[1].Trim(); // Doar mini-descrierea, fără producător!
                            }
                            else
                            {
                                categorieExtrasa = descriereCompleta;
                                descriereCurata = ""; // Sau lasă gol dacă nu există detalii după '|'
                            }

                            if (categorieSelectata != "🔍 Toate Categoriile" && !categorieExtrasa.Equals(categorieSelectata, StringComparison.OrdinalIgnoreCase))
                                continue;

                            itemiAfisare.Add(new ElementMagazin
                            {
                                IdOferta = s.Id,
                                EsteServiciu = true,
                                DenumireAfisare = s.Nume,
                                DescriereAfisare = descriereCurata,
                                PretAfisare = s.Pret,
                                CategorieAfisare = categorieExtrasa,
                                StocAfisare = "", // Serviciile nu afișează text de stoc
                                CuloareStoc = Brushes.Transparent,
                                EsteDisponibil = true,
                                ObiectOriginal = s
                            });
                        }
                    }
                    else // PRODUSE
                    {
                        var produseDinDb = context.Produse.ToList();

                        foreach (var p in produseDinDb)
                        {
                            string descriereCompleta = p.Descriere ?? "";
                            string categorieExtrasa = "Produs";
                            string descriereCurata = "";

                            if (descriereCompleta.Contains("|"))
                            {
                                var bucati = descriereCompleta.Split('|');
                                categorieExtrasa = bucati[0].Trim();
                                string miniDescriere = bucati[1].Trim();

                                if (!string.IsNullOrEmpty(miniDescriere))
                                {
                                    // Dacă are descriere, le punem pe amândouă (descrierea sus, producătorul dedesubt)
                                    descriereCurata = $"{miniDescriere}\nProducător: {p.Producator}";
                                }
                                else
                                {
                                    descriereCurata = $"Producător: {p.Producator}";
                                }
                            }
                            else
                            {
                                // Dacă nu are deloc caracterul '|', apare doar producătorul
                                categorieExtrasa = descriereCompleta.Trim();
                                descriereCurata = $"Producător: {p.Producator}";
                            }

                            if (categorieSelectata != "🔍 Toate Categoriile" && !categorieExtrasa.Equals(categorieSelectata, StringComparison.OrdinalIgnoreCase))
                                continue;

                            int stocReal = p.Stoc;
                            bool areStoc = stocReal > 0;

                            itemiAfisare.Add(new ElementMagazin
                            {
                                IdOferta = p.Id,
                                EsteServiciu = false,
                                DenumireAfisare = p.Nume,
                                DescriereAfisare = descriereCurata, // Acum va apărea doar descrierea de după |
                                PretAfisare = p.Pret,
                                CategorieAfisare = categorieExtrasa,
                                // Afișăm text doar dacă stocul este epuizat, altfel lăsăm gol ca să nu vadă clientul cantitatea
                                StocAfisare = areStoc ? "" : "❌ Stoc Epuizat",
                                CuloareStoc = areStoc ? Brushes.Transparent : Brushes.Red,
                                EsteDisponibil = areStoc,
                                ObiectOriginal = p
                            });
                        }
                    }

                    ItemsMagazin.ItemsSource = itemiAfisare;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea datelor: {ex.Message}", "Eroare DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbTipOferta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IncarcaFiltreCategorii();
            IncarcaDateMagazin();
        }

        private void CmbCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IncarcaDateMagazin();
        }


        private void BtnCumpara_Click(object sender, RoutedEventArgs e)
        {
            var buton = (Button)sender;
            if (!(buton.Tag is ElementMagazin item)) return;

            // Aflăm pe ce filtru suntem în UI
            var tipSelectat = (cmbTipOferta.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (tipSelectat == "Produse Bar & Suplimente")
            {
                // LOGICA PENTRU BAR (Produse cu stoc)
                ProceseazaCumparareProdus(item);
            }
            else
            {
                // LOGICA PENTRU SERVICII/ABONAMENTE (Fără stoc)
                ProceseazaAchizitieServiciu(item);
            }
        }
        private void ProceseazaCumparareProdus(ElementMagazin item)
        {
            // Verificăm stocul înainte de a deschide fereastra
            using (var context = new GymContext())
            {
                var produs = context.Produse.Find(item.IdOferta);
                if (produs == null || produs.Stoc <= 0)
                {
                    MessageBox.Show("Stoc epuizat!");
                    return;
                }
            }

            // Deschidem fereastra unde utilizatorul va alege cantitatea
            var fereastra = new AdaugaComandaWindow(UserSession.LoggedInUserId, item.IdOferta);
            fereastra.ShowDialog();
        }

        private void ProceseazaAchizitieServiciu(ElementMagazin item)
        {
            // NU mai facem insert direct aici! 
            // Rolul acestei metode este doar să trimită obiectul selectat către fereastra de programare/finalizare.

            var fereastraProgramare = new AdaugaProgramareWindow(item);

            // ShowDialog blochează execuția aici până când utilizatorul apasă "Finalizează" sau "Anulează"
            fereastraProgramare.ShowDialog();

            // După ce se închide fereastra, reîmprospătăm datele în magazin (dacă e nevoie)
            IncarcaDateMagazin();
        }
    }
}