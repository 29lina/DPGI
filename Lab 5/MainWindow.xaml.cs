using System;
using System.Linq;
using System.Windows;

namespace Lab_5
{
    public partial class MainWindow : Window
    {
        private StudentDirectoryEntities context;

        public MainWindow()
        {
            InitializeComponent();
            context = new StudentDirectoryEntities();
            LoadTablesData();
        }

        /// Завантаження таблиць «Студенти» і «Група»
        private void LoadTablesData()
        {
            try
            {
                dgStudents.ItemsSource = context.Студенти
                                                .Include("Група")
                                                .ToList();
                dgGroup.ItemsSource = context.Група.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження даних: " + ex.Message,
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// Кнопка "Оновити" на вкладці Студенти
        private void btnRefreshStudents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgStudents.ItemsSource = context.Студенти
                                                .Include("Група")
                                                .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка оновлення даних студентів: " + ex.Message,
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// Кнопка "Оновити" на вкладці Група
        private void btnRefreshGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgGroup.ItemsSource = context.Група.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка оновлення даних груп: " + ex.Message,
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// Пошук за номером залікової книги
        private void btnSearchByGradebook_Click(object sender, RoutedEventArgs e)
        {
            string gradebookNumber = txtGradebookNumber.Text.Trim();
            if (string.IsNullOrEmpty(gradebookNumber))
            {
                MessageBox.Show("Будь ласка, введіть номер залікової книги.",
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var query = context.Студенти
                               .Include("Група")
                               .Where(s => s.Номер_залікової_книги == gradebookNumber)
                               .ToList();

            if (query.Count == 0)
            {
                MessageBox.Show("За вказаним номером залікової книги студентів не знайдено.",
                                "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            dgQueryGradebook.ItemsSource = query;
        }

        /// Пошук за адресою
        private void btnSearchByAddress_Click(object sender, RoutedEventArgs e)
        {
            string addressPart = txtAddress.Text.Trim();
            if (string.IsNullOrEmpty(addressPart))
            {
                MessageBox.Show("Будь ласка, введіть частину адреси для пошуку.",
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var query = context.Студенти
                               .Include("Група")
                               .Where(s => s.Адреса.Contains(addressPart))
                               .ToList();

            if (query.Count == 0)
            {
                MessageBox.Show("За вказаною частиною адреси студентів не знайдено.",
                                "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            dgQueryAddress.ItemsSource = query;
        }

        private void txtAddress_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string addressPart = txtAddress.Text.Trim();

            if (string.IsNullOrEmpty(addressPart))
            {
                dgQueryAddress.ItemsSource = null;
                return;
            }

            try
            {
                var query = context.Студенти
                                   .Include("Група")
                                   .Where(s => s.Адреса.Contains(addressPart))
                                   .ToList();

                dgQueryAddress.ItemsSource = query;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка виконання запиту: " + ex.Message,
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// Радіокнопка "За кодом"
        private void rbGroupCode_Checked(object sender, RoutedEventArgs e)
        {
            // Перевірка, чи існують елементи
            if (lblGroupQuery != null)
            {
                lblGroupQuery.Content = "Код групи:";
            }
            if (txtGroupQuery != null)
            {
                txtGroupQuery.Text = string.Empty;
            }
        }

        /// Радіокнопка "За назвою"
        private void rbGroupName_Checked(object sender, RoutedEventArgs e)
        {
            if (lblGroupQuery != null)
            {
                lblGroupQuery.Content = "Назва групи:";
            }
            if (txtGroupQuery != null)
            {
                txtGroupQuery.Text = string.Empty;
            }
        }

        /// Пошук за групою (код або назва)
        private void btnSearchByGroupQuery_Click(object sender, RoutedEventArgs e)
        {
            string input = txtGroupQuery.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Будь ласка, введіть дані для пошуку.",
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IQueryable<Студенти> query = context.Студенти.Include("Група");

            if (rbGroupCode.IsChecked == true)
            {
                // Пошук за кодом групи
                int groupCode;
                if (!int.TryParse(input, out groupCode))
                {
                    MessageBox.Show("Введіть числове значення для коду групи.",
                                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                query = query.Where(s => s.Код_групи == groupCode);
            }
            else if (rbGroupName.IsChecked == true)
            {
                // Пошук за назвою групи
                query = query.Where(s => s.Група.Група1.Contains(input));
            }

            var result = query.ToList();
            if (result.Count == 0)
            {
                MessageBox.Show("За вказаними критеріями студентів не знайдено.",
                                "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            dgQueryGroup.ItemsSource = result;
        }

        /// Підрахунок студентів по групах
        private void btnCountStudents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var query = context.Database.SqlQuery<CountResult>(
                    "SELECT g.Група AS GroupName, COUNT(s.Номер_залікової_книги) AS StudentCount " +
                    "FROM [Група] g LEFT JOIN [Студенти] s ON g.Код_групи = s.Код_групи " +
                    "GROUP BY g.Група").ToList();
                dgQueryCount.ItemsSource = query;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка виконання запиту: " + ex.Message,
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// Результат підрахунку студентів по групах
    public class CountResult
    {
        public string GroupName { get; set; }
        public int StudentCount { get; set; }
    }
}
