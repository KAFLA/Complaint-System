using MySql.Data.MySqlClient;
using System;
using System.Data;
using ReklamacjeSystem.Models;

namespace ReklamacjeSystem.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(string connectionString) : base(connectionString, "Users") { }

        protected override User MapToEntity(MySqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("Id"),
                Username = reader.GetString("Username"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                Role = (UserRole)Enum.Parse(typeof(UserRole), reader.GetString("Role")), // KLUCZOWE: Upewnij się, że string z bazy jest poprawną nazwą enuma
                CreatedAt = reader.GetDateTime("CreatedAt") // KLUCZOWE: Upewnij się, że format daty w bazie pasuje
            };
        }
    }
}
