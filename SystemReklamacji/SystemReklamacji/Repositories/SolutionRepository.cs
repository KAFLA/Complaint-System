using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ReklamacjeSystem.Models; // Dodajemy referencję do modeli

namespace ReklamacjeSystem.Repositories
{
    public class SolutionRepository : BaseRepository<Solution>
    {
        public SolutionRepository(string connectionString) : base(connectionString, "Solutions") { }

        protected override Solution MapToEntity(MySqlDataReader reader)
        {
            return new Solution
            {
                Id = reader.GetInt32("Id"),
                Title = reader.GetString("Title"),
                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                Category = reader.IsDBNull("Category") ? null : reader.GetString("Category"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }

        // Metoda specjalistyczna: Wyszukiwanie rozwiązań po kategorii
        public async Task<IEnumerable<Solution>> GetSolutionsByCategoryAsync(string category)
        {
            var solutions = new List<Solution>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = $"SELECT * FROM {_tableName} WHERE Category = @Category";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Category", category);
                    // KLUCZOWA ZMIANA: Jawne rzutowanie na MySqlDataReader
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync()) // Linijka 45 (lub blisko niej)
                    {
                        while (await reader.ReadAsync())
                        {
                            solutions.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            return solutions;
        }

        // Metoda specjalistyczna: Wyszukiwanie rozwiązań po słowach kluczowych w tytule lub opisie
        public async Task<IEnumerable<Solution>> SearchSolutionsAsync(string searchTerm)
        {
            var solutions = new List<Solution>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = $"SELECT * FROM {_tableName} WHERE Title LIKE @SearchTerm OR Description LIKE @SearchTerm";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    // KLUCZOWA ZMIANA: Jawne rzutowanie na MySqlDataReader
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync()) // Linijka 69 (lub blisko niej)
                    {
                        while (await reader.ReadAsync())
                        {
                            solutions.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            return solutions;
        }
    }
}
