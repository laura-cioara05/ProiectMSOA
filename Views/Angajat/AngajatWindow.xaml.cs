using System.Windows;
using GymFitnessSystem.Views.Angajat;

namespace GymFitnessSystem.Views.Angajat
{
    public partial class AngajatWindow : Window
    {
        public AngajatWindow()
        {
            InitializeComponent();

            // Încărcăm implicit primul ecran la deschidere (Gestiune Comenzi)
            MainContentControl.Content = new UcGestiuneComenzi();
        }

        private void MeniuComenzi_Click(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new UcGestiuneComenzi();
        }

        private void MeniuCheckIn_Click(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new UcGestiuneProgramari();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Închidem sesiunea și fereastra curentă
            this.Close();
        }
    }
}