using GymFitnessSystem.Data;
using GymFitnessSystem.Models;
using System;


namespace GymFitnessSystem.Services
{
    public static class ServiciuNotificari
    {
        public static void TrimiteNotificareStatus(int utilizatorId, string tipEveniment, string statusNou, string detaliiProdus, string metodaPlata = null)
        {
            using (var context = new GymContext())
            {
                string titlu = "Notificare Sistem";
                string mesaj = string.Empty;

                switch (tipEveniment)
                {
                    case "COMANDA":
                        if (statusNou == "Disponibil la Receptie" || statusNou == "Ridicata")
                        {
                            titlu = "🛒 Comanda ta este gata!";
                            mesaj = $"Salutare! Produsul tău ({detaliiProdus}) a ajuns la recepție și poate fi ridicat. " +
                                     (metodaPlata == "Cash" ? "Nu uita că ai selectat plată Cash la recepție." : "Comanda a fost achitată cu Cardul online.");
                        }
                        else if (statusNou == "Anulata")
                        {
                            titlu = "❌ Comandă Anulată";
                            if (metodaPlata == "Card")
                            {
                                mesaj = $"Ne pare rău, dar din cauza unor complicații de stoc/transport, comanda ta pentru ({detaliiProdus}) a fost anulată. " +
                                        $"Deoarece plata a fost efectuată cu Cardul, suma va fi returnată în contul tău în termen de 2-3 zile lucrătoare.";
                            }
                            else
                            {
                                mesaj = $"Ne pare rău, dar comanda ta pentru ({detaliiProdus}) a fost anulată din motive logistice. Nu se percep taxe deoarece selectasei plata Cash.";
                            }
                        }
                        break;

                    case "PROGRAMARE":
                        if (statusNou == "Prezent")
                        {
                            titlu = "✅ Check-in Reușit!";
                            mesaj = $"Ai fost bifat ca prezent la ședința: {detaliiProdus}. Spor la antrenament!";
                        }
                        else if (statusNou == "Anulat")
                        {
                            titlu = "📅 Rezervare Anulată";
                            mesaj = $"Rezervarea ta pentru ședința ({detaliiProdus}) a fost anulată de către staff-ul sălii. Dacă consideri că este o eroare, te rugăm să contactezi recepția.";
                        }
                        break;

                    case "ABONAMENT":
                        if (statusNou == "Platit")
                        {
                            titlu = "💳 Abonament Activat!";
                            mesaj = $"Abonamentul tău ({detaliiProdus}) a fost confirmat și activat cu succes. Este valid pentru următoarele 30 de zile. Te așteptăm la sală!";
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(mesaj))
                {
                    var notificare = new Notificare
                    {
                        UtilizatorId = utilizatorId,
                        Titlu = titlu,
                        Mesaj = mesaj,
                        DataTrimitere = DateTime.Now, 
                        EsteCitit = false
                    };

                    context.Notificari.Add(notificare);
                    context.SaveChanges();
                }
            }
        }
    }
}