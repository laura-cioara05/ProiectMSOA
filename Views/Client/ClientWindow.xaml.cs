using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GymFitnessSystem.Views.Client
{
    public partial class ClientWindow : Window
    {
        public ClientWindow()
        {
            InitializeComponent();

            // Încărcăm implicit primul ecran (Shop-ul) când se deschide fereastra
            MainContentArea.Content = new UcShop();
        }

        private void Navigatie_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            // Resetăm toate butoanele la fundal transparent
            btnShop.Background = Brushes.Transparent;
            btnComenzi.Background = Brushes.Transparent;
            btnProfil.Background = Brushes.Transparent;

            // Evidențiem butonul activ și încărcăm UC-ul potrivit
            btn.Background = (Brush)new BrushConverter().ConvertFrom("#2563EB");

            if (btn == btnShop)
                MainContentArea.Content = new UcShop();
            else if (btn == btnComenzi)
                MainContentArea.Content = new UcComenzi();
            else if (btn == btnProfil)
                MainContentArea.Content = new UcProfil();
            else if (btn == btnInbox) 
                MainContentArea.Content = new UcInbox();
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}