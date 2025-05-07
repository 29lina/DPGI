using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace WpfApplProject
{
    public partial class PageHistory : Page
    {
        private enum Mode { None, New, Edit, Find, DeleteOne }
        private Mode currentMode = Mode.None;
        private List<HistoryRecord> allRecords;
        private int editId;

        public PageHistory()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            LoadAll();
        }

        private void LoadAll()
        {
            allRecords = new List<HistoryRecord>();
            var cs = GetProviderConnectionString();
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "SELECT [Id],[Приклад],[Результат],[Дата] FROM [dbo].[Історія] ORDER BY [Id]", conn))
            {
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        allRecords.Add(new HistoryRecord
                        {
                            Id = rdr.GetInt32(0),
                            Приклад = rdr.GetString(1),
                            Результат = rdr.GetString(2),
                            Дата = rdr.GetDateTime(3)
                        });
                    }
                }
            }
            HistoryDataGrid.ItemsSource = allRecords;
            ResetPanels();
        }

        private string GetProviderConnectionString()
        {
            var efcs = ConfigurationManager
               .ConnectionStrings["CalculatorEntities"].ConnectionString;
            var builder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(efcs);
            return builder.ProviderConnectionString;
        }

        private void ResetPanels()
        {
            currentMode = Mode.None;
            ActionPanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Collapsed;
            FindPanel.Visibility = Visibility.Collapsed;
            DeletePanel.Visibility = Visibility.Collapsed;
            DateSearchPanel.Visibility = Visibility.Collapsed;
            ExampleSearchText.Visibility = Visibility.Collapsed;
            ResultSearchText.Visibility = Visibility.Collapsed;

            ExampleTextBox.Text = "";
            DayText.Text = "";
            MonthText.Text = "";
            YearText.Text = "";
            HourText.Text = "";
            MinuteText.Text = "";
            ExampleSearchText.Text = "";
            ResultSearchText.Text = "";

            CommandManager.InvalidateRequerySuggested();
        }

        private void HistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => CommandManager.InvalidateRequerySuggested();

        #region CanExecute

        private void Undo_CanExecute(object s, CanExecuteRoutedEventArgs e)
            => e.CanExecute = currentMode != Mode.None;

        private void New_CanExecute(object s, CanExecuteRoutedEventArgs e)
            => e.CanExecute = currentMode == Mode.None;

        private void Replace_CanExecute(object s, CanExecuteRoutedEventArgs e)
            => e.CanExecute = currentMode == Mode.None && HistoryDataGrid?.SelectedItem != null;

        private void Save_CanExecute(object s, CanExecuteRoutedEventArgs e)
            => e.CanExecute = currentMode == Mode.New
                         || currentMode == Mode.Edit
                         || currentMode == Mode.DeleteOne;

        private void Find_CanExecute(object s, CanExecuteRoutedEventArgs e)
            => e.CanExecute = currentMode == Mode.None;

        private void Delete_CanExecute(object s, CanExecuteRoutedEventArgs e)
            => e.CanExecute = currentMode == Mode.None;

        #endregion

        #region Executed

        private void Undo_Executed(object s, ExecutedRoutedEventArgs e)
            => ResetPanels();

        private void New_Executed(object s, ExecutedRoutedEventArgs e)
        {
            currentMode = Mode.New;
            EditPanel.Visibility = Visibility.Visible;
            ActionPanel.Visibility = Visibility.Visible;
            CommandManager.InvalidateRequerySuggested();
        }

        private void Replace_Executed(object s, ExecutedRoutedEventArgs e)
        {
            if (HistoryDataGrid.SelectedItem is HistoryRecord rec)
            {
                currentMode = Mode.Edit;
                editId = rec.Id;
                ExampleTextBox.Text = rec.Приклад;
                EditPanel.Visibility = Visibility.Visible;
                ActionPanel.Visibility = Visibility.Visible;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void Save_Executed(object s, ExecutedRoutedEventArgs e)
        {
            using (var conn = new SqlConnection(GetProviderConnectionString()))
            {
                conn.Open();
                if (currentMode == Mode.New) InsertRecord(conn);
                else if (currentMode == Mode.Edit) UpdateRecord(conn);
                else if (currentMode == Mode.DeleteOne) DeleteRecord(conn);
            }
            LoadAll();
        }

        private void Find_Executed(object s, ExecutedRoutedEventArgs e)
        {
            currentMode = Mode.Find;
            FindPanel.Visibility = Visibility.Visible;
            ActionPanel.Visibility = Visibility.Visible;
            CommandManager.InvalidateRequerySuggested();
        }

        private void Delete_Executed(object s, ExecutedRoutedEventArgs e)
        {
            currentMode = Mode.DeleteOne;
            DeletePanel.Visibility = Visibility.Visible;
            ActionPanel.Visibility = Visibility.Visible;
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        private void InsertRecord(SqlConnection conn)
        {
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int newId = (int)new SqlCommand(
                        "SELECT ISNULL(MAX([Id]),0)+1 FROM [dbo].[Історія]", conn, tx
                    ).ExecuteScalar();

                    using (var cmd = new SqlCommand(
                        "INSERT INTO [dbo].[Історія]([Id],[Приклад],[Результат],[Дата]) " +
                        "VALUES(@id,@ex,@res,@dt)", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", newId);
                        cmd.Parameters.AddWithValue("@ex", ExampleTextBox.Text);

                        var expr = ExampleTextBox.Text
                            .Replace("×", "*").Replace("÷", "/").Replace("−", "-");
                        double val = Convert.ToDouble(
                            new DataTable().Compute(expr, null),
                            CultureInfo.InvariantCulture
                        );

                        cmd.Parameters.AddWithValue("@res", val.ToString(CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                catch { tx.Rollback(); }
            }
        }

        private void UpdateRecord(SqlConnection conn)
        {
            using (var cmd = new SqlCommand(
                "UPDATE [dbo].[Історія] SET [Приклад]=@ex,[Результат]=@res,[Дата]=@dt WHERE [Id]=@id",
                conn))
            {
                cmd.Parameters.AddWithValue("@id", editId);
                cmd.Parameters.AddWithValue("@ex", ExampleTextBox.Text);

                var expr = ExampleTextBox.Text
                    .Replace("×", "*").Replace("÷", "/").Replace("−", "-");
                double val = Convert.ToDouble(
                    new DataTable().Compute(expr, null),
                    CultureInfo.InvariantCulture
                );

                cmd.Parameters.AddWithValue("@res", val.ToString(CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        private void DeleteRecord(SqlConnection conn)
        {
            if (HistoryDataGrid.SelectedItem is HistoryRecord recDel)
            {
                using (var cmd = new SqlCommand(
                    "DELETE FROM [dbo].[Історія] WHERE [Id]=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", recDel.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DateRadio_Checked(object s, RoutedEventArgs e)
        {
            DateSearchPanel.Visibility = Visibility.Visible;
            ExampleSearchText.Visibility = Visibility.Collapsed;
            ResultSearchText.Visibility = Visibility.Collapsed;
        }

        private void ExampleRadio_Checked(object s, RoutedEventArgs e)
        {
            DateSearchPanel.Visibility = Visibility.Collapsed;
            ExampleSearchText.Visibility = Visibility.Visible;
            ResultSearchText.Visibility = Visibility.Collapsed;
        }

        private void ResultRadio_Checked(object s, RoutedEventArgs e)
        {
            DateSearchPanel.Visibility = Visibility.Collapsed;
            ExampleSearchText.Visibility = Visibility.Collapsed;
            ResultSearchText.Visibility = Visibility.Visible;
        }

        private void SearchFields_TextChanged(object s, TextChangedEventArgs e)
        {
            if (currentMode != Mode.Find) return;

            var filtered = allRecords.AsEnumerable();
            if (DateRadio.IsChecked == true)
            {
                filtered = filtered.Where(r =>
                {
                    var dt = r.Дата;
                    if (int.TryParse(DayText.Text, out var d) && dt.Day != d) return false;
                    if (int.TryParse(MonthText.Text, out var m) && dt.Month != m) return false;
                    if (int.TryParse(YearText.Text, out var y) && dt.Year != y) return false;
                    if (int.TryParse(HourText.Text, out var hh) && dt.Hour != hh) return false;
                    if (int.TryParse(MinuteText.Text, out var mm) && dt.Minute != mm) return false;
                    return true;
                });
            }
            else if (ExampleRadio.IsChecked == true)
            {
                var txt = ExampleSearchText.Text.ToLower();
                filtered = filtered.Where(r => r.Приклад.ToLower().StartsWith(txt));
            }
            else
            {
                var txt = ResultSearchText.Text.ToLower();
                filtered = filtered.Where(r => r.Результат.ToLower().StartsWith(txt));
            }

            HistoryDataGrid.ItemsSource = filtered.ToList();
        }

        private void HistoryDataGrid_MouseDoubleClick(object s, MouseButtonEventArgs e)
        {
            if (HistoryDataGrid.SelectedItem is HistoryRecord rec)
            {
                (Application.Current.MainWindow as MainWindow)
                    .frame1.Navigate(new PageCalculator(rec.Приклад));
            }
        }

        private void ClearAll_Click(object s, RoutedEventArgs e)
        {
            using (var conn = new SqlConnection(GetProviderConnectionString()))
            using (var cmd = new SqlCommand("DELETE FROM [dbo].[Історія]", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            LoadAll();
        }

        private void ReportExcel_Click(object s, RoutedEventArgs e)
        {
            var excelApp = new Excel.Application();
            var wb = excelApp.Workbooks.Add();
            var ws = (Excel.Worksheet)wb.Worksheets[1];
            ws.Cells[1, 1] = "Id";
            ws.Cells[1, 2] = "Приклад";
            ws.Cells[1, 3] = "Результат";
            ws.Cells[1, 4] = "Дата";
            for (int i = 0; i < allRecords.Count; i++)
            {
                var r = allRecords[i];
                int row = i + 2;
                ws.Cells[row, 1] = r.Id;
                ws.Cells[row, 2] = r.Приклад;
                ws.Cells[row, 3] = r.Результат;
                ws.Cells[row, 4] = r.Дата.ToString("dd.MM.yyyy HH:mm");
            }
            var desk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var path = GetUniqueFilePath(desk, "HistoryReport", ".xlsx");
            wb.SaveAs(path); wb.Close(); excelApp.Quit();
            MessageBox.Show($"Excel-звіт збережено:\n{path}", "Експорт");
        }

        private void ReportWord_Click(object s, RoutedEventArgs e)
        {
            var wordApp = new Word.Application();
            var doc = wordApp.Documents.Add();
            var tbl = doc.Tables.Add(doc.Range(0, 0), allRecords.Count + 1, 4);
            tbl.Borders.Enable = 1;
            tbl.Cell(1, 1).Range.Text = "Id";
            tbl.Cell(1, 2).Range.Text = "Приклад";
            tbl.Cell(1, 3).Range.Text = "Результат";
            tbl.Cell(1, 4).Range.Text = "Дата";
            for (int i = 0; i < allRecords.Count; i++)
            {
                var r = allRecords[i];
                int row = i + 2;
                tbl.Cell(row, 1).Range.Text = r.Id.ToString();
                tbl.Cell(row, 2).Range.Text = r.Приклад;
                tbl.Cell(row, 3).Range.Text = r.Результат;
                tbl.Cell(row, 4).Range.Text = r.Дата.ToString("dd.MM.yyyy HH:mm");
            }
            var desk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var path = GetUniqueFilePath(desk, "HistoryReport", ".docx");
            doc.SaveAs2(path); doc.Close(); wordApp.Quit();
            MessageBox.Show($"Word-звіт збережено:\n{path}", "Експорт");
        }

        private string GetUniqueFilePath(string folder, string name, string ext)
        {
            var path = Path.Combine(folder, name + ext);
            int cnt = 1;
            while (File.Exists(path))
                path = Path.Combine(folder, $"{name}({cnt++}){ext}");
            return path;
        }
    }

    public class HistoryRecord
    {
        public int Id { get; set; }
        public string Приклад { get; set; }
        public string Результат { get; set; }
        public DateTime Дата { get; set; }
    }
}
