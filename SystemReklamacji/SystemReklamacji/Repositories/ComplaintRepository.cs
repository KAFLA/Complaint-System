using MySql.Data.MySqlClient;
using System;
using System.Data;
using ReklamacjeSystem.Models;

namespace ReklamacjeSystem.Repositories
{
    // Repozytorium do zarządzania encjami Complaint, dziedziczące po BaseRepository
    public class ComplaintRepository : BaseRepository<Complaint>
    {
        // Konstruktor przyjmujący string połączenia, przekazuje go do klasy bazowej
        public ComplaintRepository(string connectionString) : base(connectionString, "Complaints") { }

        // Implementacja abstrakcyjnej metody MapToEntity z klasy bazowej
        // Mapuje dane z MySqlDataReader na obiekt typu Complaint
        protected override Complaint MapToEntity(MySqlDataReader reader)
        {
            return new Complaint
            {
                Id = reader.GetInt32("Id"),
                Title = reader.GetString("Title"),
                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                // KLUCZOWA ZMIANA: Użyj Enum.Parse z ignoreCase: true dla Status i Priority
                Status = (ComplaintStatus)Enum.Parse(typeof(ComplaintStatus), reader.GetString("Status"), true), // 'true' ignoruje wielkość liter
                Priority = (ComplaintPriority)Enum.Parse(typeof(ComplaintPriority), reader.GetString("Priority"), true), // 'true' ignoruje wielkość liter
                CreatedAt = reader.GetDateTime("CreatedAt"),
                // UserId może być NULL w bazie danych, więc sprawdzamy IsDBNull
                UserId = reader.IsDBNull("UserId") ? (int?)null : reader.GetInt32("UserId")
            };
        }
    }
}
