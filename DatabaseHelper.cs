using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CybersafetyChatbotPart3
{
    // Handles all database operations for storing and retrieving tasks
    public class DatabaseHelper
    {
        // Connection string - update the password to match your MySQL setup
        private string _connectionString = "Server=localhost;Database=cyberchatbot;Uid=root;Pwd=;";

        // ── Create the database and table if they don't exist ────────────────
        public void InitialiseDatabase()
        {
            try
            {
                // First connect without specifying the database to create it
                string setupConnection = "Server=localhost;Uid=root;Pwd=;";

                using (MySqlConnection conn = new MySqlConnection(setupConnection))
                {
                    conn.Open();

                    // Create the database if it doesn't exist
                    string createDb = "CREATE DATABASE IF NOT EXISTS cyberchatbot;";
                    MySqlCommand cmd = new MySqlCommand(createDb, conn);
                    cmd.ExecuteNonQuery();
                }

                // Now connect to the database and create the tasks table
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string createTable = @"
                        CREATE TABLE IF NOT EXISTS tasks (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            title VARCHAR(255) NOT NULL,
                            description TEXT,
                            reminder VARCHAR(255),
                            is_completed BOOLEAN DEFAULT FALSE,
                            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                        );";

                    MySqlCommand cmd = new MySqlCommand(createTable, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // If database setup fails, log the error
                Console.WriteLine("Database setup error: " + ex.Message);
            }
        }

        // ── Add a new task to the database ───────────────────────────────────
        public bool AddTask(string title, string description, string? reminder)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = "INSERT INTO tasks (title, description, reminder) VALUES (@title, @description, @reminder);";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@reminder", reminder ?? "");
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ── Get all tasks from the database ──────────────────────────────────
        public List<TaskItem> GetAllTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = "SELECT id, title, description, reminder, is_completed, created_at FROM tasks ORDER BY created_at DESC;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        TaskItem task = new TaskItem
                        {
                            Id = reader.GetInt32("id"),
                            Title = reader.GetString("title"),
                            Description = reader.GetString("description"),
                            Reminder = reader.IsDBNull(reader.GetOrdinal("reminder")) ? null : reader.GetString("reminder"),
                            IsCompleted = reader.GetBoolean("is_completed"),
                            CreatedAt = reader.GetDateTime("created_at")
                        };
                        tasks.Add(task);
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list if database is unavailable
            }

            return tasks;
        }

        // ── Mark a task as completed ─────────────────────────────────────────
        public bool CompleteTask(int taskId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE tasks SET is_completed = TRUE WHERE id = @id;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ── Delete a task from the database ──────────────────────────────────
        public bool DeleteTask(int taskId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM tasks WHERE id = @id;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}