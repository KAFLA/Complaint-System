using System;
using System.Threading.Tasks;
using ReklamacjeSystem.Models; // Jeśli powiadomienia będą dotyczyć konkretnych modeli

namespace ReklamacjeSystem.Services
{
    // Usługa odpowiedzialna za generowanie i obsługę powiadomień
    public class NotificationService
    {
        // Metoda do wysyłania ogólnego powiadomienia
        public async Task SendNotification(string message, string recipient = "all")
        {
            // Tutaj można by zaimplementować wysyłanie powiadomień do UI,
            // logowanie do pliku, wysyłanie e-maili itp.
            await Task.Run(() =>
            {
                Console.WriteLine($"[Powiadomienie do {recipient}]: {message} o {DateTime.Now}");
                // W prawdziwej aplikacji, to by była złożona logika powiadomień
            });
        }

        // Metoda wysyłająca powiadomienie o zmianie statusu reklamacji
        public async Task NotifyComplaintStatusChange(Complaint complaint)
        {
            string message = $"Status reklamacji '{complaint.Title}' (ID: {complaint.Id}) zmieniono na: {complaint.Status}.";
            // Można dodać logikę wysyłania do konkretnego użytkownika (np. zgłaszającego)
            await SendNotification(message, "Zgłaszający / Odpowiedzialny");
        }
    }
}
