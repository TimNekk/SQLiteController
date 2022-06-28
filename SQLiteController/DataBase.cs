using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace SQLiteController
{
    public class DataBase : IDataBase
    {
        private readonly SQLiteConnection _connection;
        
        public DataBase(FileSystemInfo path)
        {
            _connection = new SQLiteConnection($"Data Source={path.FullName};Version=3;");
            _connection.Open();
        }

        // Get all available tables names from database
        public ICollection<string> GetTablesNames()
        {
            var tables = new List<string>();
            var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table'", _connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(0) != "sqlite_sequence")
                {
                    tables.Add(reader.GetString(0));
                }
            }
            return tables;
        }

        // Get all available columns names from table
        public ICollection<string> GetColumnsNames(string tableName)
        {
            var columns = new List<string>();
            var command = new SQLiteCommand($"PRAGMA table_info({tableName})", _connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                columns.Add(reader.GetString(1));
            }
            return columns;
        }
        
        // Get all available values from column
        public ICollection<string> GetData(string tableName, string columnName)
        {
            var data = new List<string>();
            var command = new SQLiteCommand($"SELECT {columnName} FROM {tableName}", _connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                data.Add(reader.GetValue(0).ToString());
            }
            return data;
        }
        
        // Change value if other value equal to new value
        public void CheckAndChangeValues(
            string tableName, 
            string checkColumnName, 
            string editColumnName, 
            string valueIfContains, 
            string valueIfNotContains,
            IEnumerable<string> checkValues)
        {
            var joinedCheckValues = string.Join(",", checkValues);
            var commandIfContains = new SQLiteCommand(
                $"UPDATE {tableName} SET {editColumnName} = '{valueIfContains}' WHERE {checkColumnName} IN ({joinedCheckValues})",
                _connection);
            var commandIfNotContains = new SQLiteCommand(
                $"UPDATE {tableName} SET {editColumnName} = '{valueIfNotContains}' WHERE {checkColumnName} NOT IN ({joinedCheckValues})",
                _connection);
            commandIfContains.ExecuteNonQuery();
            commandIfNotContains.ExecuteNonQuery();
        }
    }
}