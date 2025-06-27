using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq; // Dodajemy to do użycia .Where() i .Select()
using System.Reflection;
using System.Threading.Tasks;
using ReklamacjeSystem.Models; // Dodajemy referencję do modeli

namespace ReklamacjeSystem.Repositories
{
    // Klasa bazowa dla repozytoriów, implementująca generyczny interfejs IRepository
    public abstract class BaseRepository<T> : IRepository<T> where T : class, new()
    {
        protected readonly string _connectionString; // String połączenia do bazy danych
        protected readonly string _tableName; // Nazwa tabeli w bazie danych

        // Konstruktor przyjmujący string połączenia i nazwę tabeli
        public BaseRepository(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        // Metoda do tworzenia połączenia z bazą danych
        protected MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        // Pobiera wszystkie encje z tabeli
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var entities = new List<T>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = $"SELECT * FROM {_tableName}";
                using (var command = new MySqlCommand(query, connection))
                // Tutaj jest kluczowa zmiana: jawne rzutowanie na MySqlDataReader
                using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entities.Add(MapToEntity(reader));
                    }
                }
            }
            return entities;
        }

        // Pobiera encję po ID
        public async Task<T> GetByIdAsync(int id)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = $"SELECT * FROM {_tableName} WHERE Id = @Id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    // Tutaj jest kluczowa zmiana: jawne rzutowanie na MySqlDataReader
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapToEntity(reader);
                        }
                    }
                }
            }
            return null; // Zwraca null, jeśli encja nie została znaleziona
        }

        // Dodaje nową encję do tabeli
        public async Task AddAsync(T entity)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                // Poprawione pobieranie właściwości: Ignorujemy 'Id' (auto-increment),
                // oraz właściwości nawigacyjne. Uwzględniamy tylko właściwości, które mają publiczny getter i setter,
                // i które są typami wartościowymi, stringami LUB ENUMAMI.
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                          .Where(p => p.Name != "Id" && p.CanRead && p.CanWrite &&
                                                      (!p.PropertyType.IsClass || p.PropertyType == typeof(string) || p.PropertyType.IsEnum)); // Dodano || p.PropertyType.IsEnum

                var columns = string.Join(", ", properties.Select(p => p.Name));
                var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));

                var query = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters})";
                using (var command = new MySqlCommand(query, connection))
                {
                    foreach (var prop in properties)
                    {
                        object value = prop.GetValue(entity);
                        // Specjalna obsługa dla enumów: konwertuj na stringa, aby zapisać w bazie danych
                        if (prop.PropertyType.IsEnum)
                        {
                            value = value.ToString();
                        }
                        command.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
                    }
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Aktualizuje istniejącą encję w tabeli
        public async Task UpdateAsync(T entity)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                // Poprawione pobieranie właściwości: Podobnie jak w AddAsync, uwzględniamy enums.
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                          .Where(p => p.Name != "Id" && p.CanRead && p.CanWrite &&
                                                      (!p.PropertyType.IsClass || p.PropertyType == typeof(string) || p.PropertyType.IsEnum)); // Dodano || p.PropertyType.IsEnum

                var setClauses = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

                var query = $"UPDATE {_tableName} SET {setClauses} WHERE Id = @Id";
                using (var command = new MySqlCommand(query, connection))
                {
                    foreach (var prop in properties)
                    {
                        object value = prop.GetValue(entity);
                        // Specjalna obsługa dla enumów: konwertuj na stringa
                        if (prop.PropertyType.IsEnum)
                        {
                            value = value.ToString();
                        }
                        command.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
                    }
                    var idProperty = typeof(T).GetProperty("Id");
                    if (idProperty != null)
                    {
                        command.Parameters.AddWithValue("@Id", idProperty.GetValue(entity));
                    }
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Usuwa encję z tabeli po ID
        public async Task DeleteAsync(int id)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = $"DELETE FROM {_tableName} WHERE Id = @Id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Abstrakcyjna metoda do mapowania danych z MySqlDataReader na obiekt encji T
        // Musi być zaimplementowana w klasach dziedziczących
        protected abstract T MapToEntity(MySqlDataReader reader);
    }
}
