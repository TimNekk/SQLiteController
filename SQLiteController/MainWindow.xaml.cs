using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SQLiteController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private FileSystemInfo _dataBaseDirectory;
        private FileSystemInfo DataBaseDirectory
        {
            get => _dataBaseDirectory;
            set
            {
                _dataBaseDirectory = value;
                InitializeData();
                DataBase = new DataBase(DataBaseDirectory);
                DataBaseDirectoryTextBox.Text = DataBaseDirectory.FullName;
            }
        }

        private IDataBase _dataBase;
        private IDataBase DataBase
        {
            get => _dataBase;
            set
            {
                _dataBase = value;
                SetItemsOfListBox(ExportTableListBox, DataBase.GetTablesNames());
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

        private const string DataBaseFileFilter = "SQLite|*.db; *.sdb; *.sqlite; *.db3; *.s3db; *.sqlite3; *.sl3|" + "Все файлы|*";
        private const string NotSelected = "Не выбрано";

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
        }
        
        private void InitializeData()
        {
            _exportTable = NotSelected;
            ExportTableLabel.Text = ExportTable;
            ExportTableListBox.Items.Clear();
            
            _exportColumn = NotSelected;
            ExportColumnLabel.Text = ExportColumn;
            ExportColumnListBox.Items.Clear();
        }

        private void SelectDataBaseButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = DataBaseFileFilter
            };

            var result = dialog.ShowDialog();

            if (result == true)
            {
                DataBaseDirectory = new FileInfo(dialog.FileName);
            }
        }

        private void ExportTableListBoxSelected(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox) sender;
            var selectedItem = (ListBoxItem) listBox.SelectedItem;
            if (selectedItem == null) {
                _exportTable = NotSelected;
                ExportTableLabel.Text = ExportTable;
                return;
            }
            ExportTable = selectedItem.Content.ToString();
        }

        private void ExportColumnListBoxSelected(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox) sender;
            var selectedItem = (ListBoxItem) listBox.SelectedItem;
            if (selectedItem == null) {
                _exportColumn = NotSelected;
                ExportColumnLabel.Text = ExportColumn;
                return;
            }
            ExportColumn = selectedItem.Content.ToString();
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
                MessageBox.Show("Не выбраны таблица или столбец");
                return;
            }
            
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстовый файл|*.txt",
                FileName = $"{ExportTable}_{ExportColumn}"
            };
            
            var result = dialog.ShowDialog();

            if (result != true) return;
            
            var fileInfo = new FileInfo(dialog.FileName);
            var fileStream = fileInfo.Create();
            var writer = new StreamWriter(fileStream);
            var data = DataBase.GetData(ExportTable, ExportColumn);
            foreach (var row in data)
            {
                writer.WriteLine(row);
            }
            writer.Close();
            fileStream.Close();

            
            MessageBox.Show($"Выгружено строк: {data.Count()}", "Выгрузка завершена");
        }
    }
}