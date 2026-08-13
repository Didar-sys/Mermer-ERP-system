using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Text.Json;

namespace Mermer.Ui.Pc.Services
{
    public static class LocalSqliteCache
    {
        private static string DbPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mermer_local.db");
        public static string ConnectionString => $"Data Source={DbPath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // Универсальная NoSQL таблица для всех документов системы
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS local_docs (
                            doc_type TEXT,
                            id TEXT,
                            json_data TEXT,
                            is_synced INTEGER DEFAULT 0,
                            updated_at TEXT,
                            PRIMARY KEY (doc_type, id)
                        );
                    ";
                    command.ExecuteNonQuery();
                }
            }
        }

        // --- УНИВЕРСАЛЬНЫЕ МЕТОДЫ ЧТЕНИЯ И ЗАПИСИ ---

        public static void SaveDocument<T>(string docType, string id, T entity, bool isSynced)
        {
            if (entity == null || string.IsNullOrEmpty(id)) return;

            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO local_docs (doc_type, id, json_data, is_synced, updated_at) 
                            VALUES (@docType, @id, @jsonData, @isSynced, @updatedAt)
                            ON CONFLICT(doc_type, id) DO UPDATE SET
                                json_data = @jsonData,
                                is_synced = @isSynced,
                                updated_at = @updatedAt;
                        ";

                        command.Parameters.AddWithValue("@docType", docType);
                        command.Parameters.AddWithValue("@id", id);
                        var jsonOptions = new JsonSerializerOptions
                        {
                            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                        };
                        command.Parameters.AddWithValue("@jsonData", JsonSerializer.Serialize(entity, jsonOptions));
                        command.Parameters.AddWithValue("@isSynced", isSynced ? 1 : 0);
                        command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("o"));

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLite Save Error - {docType}]: {ex.Message}");
            }
        }

        public static List<(string id, T entity)> GetUnsyncedDocuments<T>(string docType)
        {
            var result = new List<(string id, T entity)>();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT id, json_data FROM local_docs WHERE doc_type = @docType AND is_synced = 0;";
                        command.Parameters.AddWithValue("@docType", docType);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string id = reader.GetString(0);
                                string json = reader.GetString(1);
                                if (!string.IsNullOrEmpty(json))
                                {
                                    try
                                    {
                                        var entity = JsonSerializer.Deserialize<T>(json);
                                        if (entity != null) result.Add((id, entity));
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLite GetUnsynced Error - {docType}]: {ex.Message}");
            }
            return result;
        }

        public static List<T> GetAllDocuments<T>(string docType)
        {
            var result = new List<T>();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT json_data FROM local_docs WHERE doc_type = @docType;";
                        command.Parameters.AddWithValue("@docType", docType);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string json = reader.GetString(0);
                                if (!string.IsNullOrEmpty(json))
                                {
                                    try
                                    {
                                        var entity = JsonSerializer.Deserialize<T>(json);
                                        if (entity != null) result.Add(entity);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLite Read Error - {docType}]: {ex.Message}");
            }
            return result;
        }
    }
}