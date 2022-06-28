using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SQLiteController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private FileSystemInfo _dataBaseFileInfo;
        private FileSystemInfo DataBaseFileInfo
        {
            get => _dataBaseFileInfo;
            set
            {
                _dataBaseFileInfo = value;
                ResetAllListBoxes();
                DataBase = new DataBase(DataBaseFileInfo);
                DataBaseFileTextBox.Text = DataBaseFileInfo.FullName;
            }
        }
        
        private FileSystemInfo _importFileInfo;
        private FileSystemInfo ImportFileInfo
        {
            get => _importFileInfo;
            set
            {
                _importFileInfo = value;
                ImportFileTextBox.Text = ImportFileInfo.FullName;
            }
        }

        private IDataBase _dataBase;
        private IDataBase DataBase
        {
            get => _dataBase;
            set
            {
                _dataBase = value;
                var tables = DataBase.GetTablesNames();
                SetItemsOfListBox(ExportTableListBox, tables);
                SetItemsOfListBox(ImportTableListBox, tables);
            }
        }
        
        private string _exportTable;
        private string ExportTable
        {
            get => _exportTable;
            set
            {
                _exportTable = value;
                ExportTableLabel.Text = ExportTable;
                SetItemsOfListBox(ExportColumnListBox, DataBase.GetColumnsNames(ExportTable));
            }
        }
        
        private string _exportColumn;
        private string ExportColumn
        {
            get => _exportColumn;
            set
            {
                if (value is null)
                {
                    ExportColumnLabel.Text = "Не выбрано"; 
                    return;
                }
                _exportColumn = value;
                ExportColumnLabel.Text = ExportColumn;
            }
        }
        
        private string _importTable;
        private string ImportTable
        {
            get => _importTable;
            set
            {
                _importTable = value;
                ImportTableLabel.Text = ImportTable;
                var columns = DataBase.GetColumnsNames(ImportTable);
                SetItemsOfListBox(ImportColumnCheckListBox, columns);
                SetItemsOfListBox(ImportColumnEditListBox, columns);
            }
        }
        
        private string _importColumnCheck;
        private string ImportColumnCheck
        {
            get => _importColumnCheck;
            set
            {
                if (value is null)
                {
                    ImportColumnCheckLabel.Text = "Не выбрано"; 
                    return;
                }
                _importColumnCheck = value;
                ImportColumnCheckLabel.Text = ImportColumnCheck;
            }
        }
        
        private string _importColumnEdit;
        private string ImportColumnEdit
        {
            get => _importColumnEdit;
            set
            {
                if (value is null)
                {
                    ImportColumnEditLabel.Text = "Не выбрано"; 
                    return;
                }
                _importColumnEdit = value;
                ImportColumnEditLabel.Text = ImportColumnEdit;
            }
        }

        private const string AllFilesFilter = "Все файлы|*";
        private const string DataBaseFilesFilter = "SQLite|*.db; *.sdb; *.sqlite; *.db3; *.s3db; *.sqlite3; *.sl3|" + AllFilesFilter;
        private const string ImportFilesFilter = "Текстовые файлы|*.txt|" + AllFilesFilter;
        private const string NotSelected = "Не выбрано";

        public MainWindow()
        {
            InitializeComponent();
            ResetAllListBoxes();
        }

        private static void ResetListBox(ListBox listBox, TextBlock textBlock, out string variableName)
        {
            variableName = NotSelected;
            textBlock.Text = variableName;
            listBox.Items.Clear();
        }

        private void ResetAllListBoxes()
        {
            ResetListBox(ExportTableListBox, ExportTableLabel, out _exportTable);
            ResetListBox(ExportColumnListBox, ExportColumnLabel, out _exportColumn);
            ResetListBox(ImportTableListBox, ImportTableLabel, out _importTable);
            ResetListBox(ImportColumnCheckListBox, ImportColumnCheckLabel, out _importColumnCheck);
            ResetListBox(ImportColumnEditListBox, ImportColumnEditLabel, out _importColumnEdit);
        }

        private void SelectFileButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var filter = "";
            switch (button.Name)
            {
                case nameof(DataBaseFileSelectButton):
                    filter = DataBaseFilesFilter;
                    break;
                case nameof(ImportFileSelectButton):
                    filter = ImportFilesFilter;
                    break;
            }
            
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter
            };

            if (!(dialog.ShowDialog() is true)) return;
            
            var file = new FileInfo(dialog.FileName);
            switch (button.Name)
            {
                case nameof(DataBaseFileSelectButton):
                    DataBaseFileInfo = file;
                    break;
                case nameof(ImportFileSelectButton):
                    ImportFileInfo = file;
                    break;
            }
        }
        
        private void ListBoxSelected(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox) sender;
            var selectedItem = (ListBoxItem) listBox.SelectedItem;

            switch (listBox.Name)
            {
                case nameof(ExportTableListBox):
                    if (selectedItem is null) ResetListBox(listBox, ExportTableLabel, out _exportTable);
                    else { ExportTable = selectedItem.Content.ToString(); }
                    break;
                case nameof(ExportColumnListBox):
                    if (selectedItem is null) ResetListBox(listBox, ExportColumnLabel, out _exportColumn);
                    else { ExportColumn = selectedItem.Content.ToString(); }
                    break;
                case nameof(ImportTableListBox):
                    if (selectedItem is null) ResetListBox(listBox, ImportTableLabel, out _importTable);
                    else { ImportTable = selectedItem.Content.ToString(); }
                    break;
                case nameof(ImportColumnCheckListBox):
                    if (selectedItem is null) ResetListBox(listBox, ImportColumnCheckLabel, out _importColumnCheck);
                    else { ImportColumnCheck = selectedItem.Content.ToString(); }
                    break;
                case nameof(ImportColumnEditListBox):
                    if (selectedItem is null) ResetListBox(listBox, ImportColumnEditLabel, out _importColumnEdit);
                    else { ImportColumnEdit = selectedItem.Content.ToString(); }
                    break;
            }
        }

        private static void SetItemsOfListBox(ListBox listBox, IEnumerable<string> items)
        {
            listBox.Items.Clear();
            foreach (var item in items)
            {
                listBox.Items.Add(new ListBoxItem {Content = item});
            }
        }

        private void ExportButtonClick(object sender, RoutedEventArgs e)
        {
            if (ExportTable == NotSelected || ExportColumn == NotSelected)
            {
                MessageBox.Show("Не выбраны таблица или столбец", "Ошибка");
                return;
            }
            
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстовый файл|*.txt",
                FileName = $"{ExportTable}_{ExportColumn}"
            };
            
            if (dialog.ShowDialog() is false) return;
            
            using var streamWriter = new StreamWriter(dialog.FileName);
            var data = DataBase.GetData(ExportTable, ExportColumn);
            foreach (var row in data)
            {
                streamWriter.WriteLine(row);
            }

            MessageBox.Show($"Выгружено строк: {data.Count}", "Выгрузка завершена");
        }

        private void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            if (ImportTable == NotSelected || ImportColumnCheck == NotSelected || ImportColumnEdit == NotSelected)
            {
                MessageBox.Show("Не выбраны таблица или столбец для проверки или редактирования", "Ошибка");
                return;
            }
            
            var (valueIfContains, valueIfNotContains) = (ValueIfContainsTextBox.Text, ValueIfNotContainsTextBox.Text);
            if (valueIfContains == string.Empty || valueIfNotContains == string.Empty || ImportFileInfo is null)
            {
                MessageBox.Show("Не выбраны файл с данными или значения \"Если в файле\" или \"Если нет\"", "Ошибка");
                return;
            }

            using var streamReader = new StreamReader(ImportFileInfo.FullName);
            var checkValues = streamReader.ReadToEnd().Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            
            DataBase.CheckAndChangeValues(ImportTable, ImportColumnCheck, ImportColumnEdit, valueIfContains, valueIfNotContains, checkValues);
            
            MessageBox.Show("Импорт завершен", "Импорт завершен");
        }

    }
}