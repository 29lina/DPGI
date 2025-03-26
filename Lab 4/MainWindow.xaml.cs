using System;
using System.Data;
using System.Windows;

namespace Lab_4
{
    public partial class MainWindow : Window
    {
        private AdoAssistant ado = new AdoAssistant();

        private enum FormMode { View, Create, Edit }
        private FormMode currentMode = FormMode.View;

        private string originalNzk = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            ado.RefreshStudentsTable();
            DataTable dt = ado.LoadStudentsTable();
            listStudents.DataContext = dt;

            SetMode(FormMode.View);

            if (listStudents.Items.Count > 0)
            {
                listStudents.SelectedIndex = 0;
                listStudents.IsEnabled = true;
                listStudents.Focus();
            }
        }

        private void SetMode(FormMode mode)
        {
            currentMode = mode;
            switch (mode)
            {
                case FormMode.View:
                    btnCancel.Visibility = Visibility.Collapsed;
                    btnCreate.IsEnabled = true;
                    btnUpdate.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    listStudents.IsEnabled = true;
                    tbNomer.IsReadOnly = true;
                    tbGrupa.IsReadOnly = true;
                    tbAddress.IsReadOnly = true;
                    lblPIB.Visibility = Visibility.Collapsed;
                    tbPIB.Visibility = Visibility.Collapsed;
                    break;
                case FormMode.Create:
                    btnCancel.Visibility = Visibility.Visible;
                    btnCreate.IsEnabled = true;
                    btnUpdate.IsEnabled = false;
                    btnDelete.IsEnabled = false;
                    listStudents.IsEnabled = false;
                    listStudents.SelectedItem = null;
                    tbNomer.Text = "";
                    tbPIB.Text = "";
                    tbGrupa.Text = "";
                    tbAddress.Text = "";
                    tbNomer.IsReadOnly = false;
                    tbGrupa.IsReadOnly = false;
                    tbAddress.IsReadOnly = false;
                    lblPIB.Visibility = Visibility.Visible;
                    tbPIB.Visibility = Visibility.Visible;
                    originalNzk = null;
                    break;
                case FormMode.Edit:
                    btnCancel.Visibility = Visibility.Visible;
                    btnCreate.IsEnabled = false;
                    btnUpdate.IsEnabled = true;
                    btnDelete.IsEnabled = false;
                    listStudents.IsEnabled = false;
                    tbNomer.IsReadOnly = false;
                    tbGrupa.IsReadOnly = false;
                    tbAddress.IsReadOnly = false;
                    lblPIB.Visibility = Visibility.Visible;
                    tbPIB.Visibility = Visibility.Visible;
                    originalNzk = tbNomer.Text;
                    break;
            }
        }

        private bool ValidateInput(out string errorMessage)
        {
            errorMessage = "";
            if (string.IsNullOrWhiteSpace(tbNomer.Text) ||
                string.IsNullOrWhiteSpace(tbPIB.Text) ||
                string.IsNullOrWhiteSpace(tbGrupa.Text) ||
                string.IsNullOrWhiteSpace(tbAddress.Text))
            {
                errorMessage = "Будь ласка, заповніть усі поля.";
                return false;
            }

            string nomer = tbNomer.Text.Trim();
            if (nomer.Length > 10)
            {
                errorMessage = "Номер залікової книжки повинен містити не більше 10 символів.";
                return false;
            }
            string pib = tbPIB.Text.Trim();
            if (pib.Length > 100)
            {
                errorMessage = "ПІБ повинен містити не більше 100 символів.";
                return false;
            }
            string grupa = tbGrupa.Text.Trim();
            if (grupa.Length > 10)
            {
                errorMessage = "Група повинна містити не більше 10 символів.";
                return false;
            }

            DataTable dt = listStudents.DataContext as DataTable;
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string existing = row["Номер_залікової_книги"].ToString().Trim();
                    if (currentMode == FormMode.Create)
                    {
                        if (string.Equals(existing, nomer, StringComparison.OrdinalIgnoreCase))
                        {
                            errorMessage = "Запис з таким номером залікової книжки вже існує.";
                            return false;
                        }
                    }
                    else if (currentMode == FormMode.Edit)
                    {
                        if (!string.Equals(existing, originalNzk, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(existing, nomer, StringComparison.OrdinalIgnoreCase))
                        {
                            errorMessage = "Запис з таким номером залікової книжки вже існує.";
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == FormMode.View)
            {
                SetMode(FormMode.Create);
            }
            else if (currentMode == FormMode.Create)
            {
                if (!ValidateInput(out string errorMsg))
                {
                    MessageBox.Show(errorMsg, "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                try
                {
                    ado.CreateStudent(
                        tbNomer.Text.Trim(),
                        tbPIB.Text.Trim(),
                        tbGrupa.Text.Trim(),
                        tbAddress.Text.Trim()
                    );
                    MessageBox.Show("Новий запис успішно створено!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshData();
                }
                catch (Exception)
                {
                    MessageBox.Show("Сталася непередбачена помилка при створенні запису.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Спершу завершіть поточну операцію редагування.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == FormMode.View && listStudents.SelectedItem != null)
            {
                SetMode(FormMode.Edit);
            }
            else if (currentMode == FormMode.Edit)
            {
                if (!ValidateInput(out string errorMsg))
                {
                    MessageBox.Show(errorMsg, "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                try
                {
                    ado.UpdateStudent(
                        oldNomerZalik: originalNzk,
                        newNomerZalik: tbNomer.Text.Trim(),
                        pib: tbPIB.Text.Trim(),
                        grupa: tbGrupa.Text.Trim(),
                        address: tbAddress.Text.Trim()
                    );
                    MessageBox.Show("Запис успішно оновлено!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshData();
                }
                catch (Exception)
                {
                    MessageBox.Show("Сталася непередбачена помилка при оновленні запису.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Спершу завершіть поточну операцію створення.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listStudents.SelectedItem == null)
            {
                MessageBox.Show("Не вибрано запис для видалення.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (MessageBox.Show("Ви впевнені, що хочете видалити цей запис?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    ado.DeleteStudent(tbNomer.Text.Trim());
                    MessageBox.Show("Запис успішно видалено!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshData();
                }
                catch (Exception)
                {
                    MessageBox.Show("Сталася помилка при видаленні запису.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            SetMode(FormMode.View);
            RefreshData();
        }
    }
}
