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
        public IEnumerable<string> GetTablesNames()
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
        public IEnumerable<string> GetColumnsNames(string tableName)
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
        public IEnumerable<object> GetData(string tableName, string columnName)
        {
            var data = new List<object>();
            var command = new SQLiteCommand($"SELECT {columnName} FROM {tableName}", _connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                data.Add(reader.GetValue(0));
            }
            return data;
        }
    }
}