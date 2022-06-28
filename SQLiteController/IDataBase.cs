using System.Collections.Generic;

namespace SQLiteController
{
    public interface IDataBase
    {
        ICollection<string> GetTablesNames();
        
        ICollection<string> GetColumnsNames(string tableName);
        
        ICollection<string> GetData(string tableName, string columnName);

        void CheckAndChangeValues(
            string tableName,
            string checkColumnName,
            string editColumnName,
            string valueIfContains,
            string valueIfNotContains,
            IEnumerable<string> checkValues);
    }
}