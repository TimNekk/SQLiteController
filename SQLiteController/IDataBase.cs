using System.Collections.Generic;

namespace SQLiteController
{
    public interface IDataBase
    {
        IEnumerable<string> GetTablesNames();
        
        IEnumerable<string> GetColumnsNames(string tableName);
        
        IEnumerable<object> GetData(string tableName, string columnName);
    }
}