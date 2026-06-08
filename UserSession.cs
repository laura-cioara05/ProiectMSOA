using System;
using System.Collections.Generic;
using System.Text;

namespace GymFitnessSystem.Services
{
    public static class UserSession
    {
        // Aici se păstrează ID-ul utilizatorului după logare
        public static int LoggedInUserId { get; set; }
        public static string Username { get; set; }

        public static void Clear()
        {
            LoggedInUserId = 0;
            Username = string.Empty;
        }

    }
}
