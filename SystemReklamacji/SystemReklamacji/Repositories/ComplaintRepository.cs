using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ReklamacjeSystem.Models;

namespace ReklamacjeSystem.Repositories
{
    public class ComplaintRepository : BaseRepository<Complaint>
    {
        public ComplaintRepository(string connectionString) : base(connectionString, "Complaints") { }

        protected override Complaint MapToEntity(MySqlDataReader reader)
        {
            return new Complaint
            {
                Id = reader.GetInt32("Id"),
                Title = reader.GetString("Title"),
                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                Status = (ComplaintStatus)Enum.Parse(typeof(ComplaintStatus), reader.GetString("Status")),
                Priority = (ComplaintPriority)Enum.Parse(typeof(ComplaintPriority), reader.GetString("Priority")),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UserId = reader.IsDBNull("UserId") ? (int?)null : reader.GetInt32("UserId")
            };
        }

        // Metoda specjalistyczna: Pobiera reklamacje po statusie
        public async Task<IEnumerable<Complaint>> GetComplaintsByStatusAsync(ComplaintStatus status)
        {
            var complaints = new List<Complaint>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = $"SELECT * FROM {_tableName} WHERE Status = @Status";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Status", status.ToString());
                    // KLUCZOWA ZMIANA: Jawne rzutowanie na MySqlDataReader
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            complaints.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            return complaints;
        }

        // Metoda specjalistyczna: Pobiera reklamacje przypisane do konkretnego użytkownika
        public async Task<IEnumerable<Complaint>> GetComplaintsByUserIdAsync(int userId)
        {
            var complaints = new List<Complaint>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = $"SELECT * FROM {_tableName} WHERE UserId = @UserId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    // KLUCZOWA ZMIANA: Jawne rzutowanie na MySqlDataReader
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            complaints.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            return complaints;
        }
    }
}
