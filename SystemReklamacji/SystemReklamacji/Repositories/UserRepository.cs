using MySql.Data.MySqlClient;
using System;
using System.Data;
using ReklamacjeSystem.Models; // Dodajemy referencję do modeli

namespace ReklamacjeSystem.Repositories
{
    // Repozytorium dla encji User, dziedziczy po BaseRepository
    public class UserRepository : BaseRepository<User>
    {
        // Konstruktor, przekazuje connectionString i nazwę tabeli do klasy bazowej
        public UserRepository(string connectionString) : base(connectionString, "Users") { }

        // Implementacja abstrakcyjnej metody MapToEntity dla obiektu User
        protected override User MapToEntity(MySqlDataReader reader)
        {
            // Mapowanie danych z readera na obiekt User
            return new User
            {
                Id = reader.GetInt32("Id"),
                Username = reader.GetString("Username"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                Role = (UserRole)Enum.Parse(typeof(UserRole), reader.GetString("Role")), // Konwersja stringa na enum UserRole
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }
    }
}
