using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CalculatorWPF
{
    public partial class MainWindow : Window
    {
        // Поточний вираз, введений користувачем
        private string expression = "";
        // Прапорець для чергування відкритої/закритої дужки
        private bool isOpenParenthesis = true;

        //Конструктор: ініціалізація компонентів
        public MainWindow()
        {
            InitializeComponent();
        }

        // Подія завантаження вікна: встановлюємо фокус
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        // Оновлює текст на дисплеї калькулятора
        private void UpdateDisplay()
        {
            Display.Text = string.IsNullOrEmpty(expression) ? "0" : expression;
        }

        // Обробник натискання цифр: додає цифру в вираз
        private void Number_Click(object sender, RoutedEventArgs e)
        {
            expression += (sender as Button).Content;
            UpdateDisplay();
        }

        // Обробник точки: додає крапку до виразу
        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            expression += ".";
            UpdateDisplay();
        }

        // Обробник операторів (+, −, ×, ÷): додає оператор до виразу
        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            expression += (sender as Button).Content;
            UpdateDisplay();
        }

        // Обробник дужок: чергує відкриття та закриття
        private void Parentheses_Click(object sender, RoutedEventArgs e)
        {
            if (isOpenParenthesis) expression += "(";
            else expression += ")";
            isOpenParenthesis = !isOpenParenthesis;
            UpdateDisplay();
        }

        // Обробник Backspace: видаляє останній символ
        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (expression.Length > 0)
                expression = expression.Substring(0, expression.Length - 1);
            UpdateDisplay();
        }

        // Обробник Clear: очищає вираз
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            expression = "";
            isOpenParenthesis = true;
            UpdateDisplay();
        }

        // Обробник Negate: змінює знак останнього числа
        private void Negate_Click(object sender, RoutedEventArgs e)
        {
            // Знаходимо початок останнього числового токена
            int i = expression.Length - 1;
            while (i >= 0 && (char.IsDigit(expression[i]) || expression[i] == '.'))
                i--;
            int tokenStart = i + 1;

            // Якщо вже є "(-" — видаляємо, інакше вставляємо
            if (tokenStart >= 2
                && expression[tokenStart - 2] == '('
                && expression[tokenStart - 1] == '-')
            {
                expression = expression.Remove(tokenStart - 2, 2);
                isOpenParenthesis = true;
            }
            else
            {
                expression = expression.Insert(tokenStart, "(-");
                isOpenParenthesis = false;
            }
            UpdateDisplay();
        }

        // Обробник відсотків: додає символ '%'
        private void Percent_Click(object sender, RoutedEventArgs e)
        {
            expression += "%";
            UpdateDisplay();
        }

        // Обчислення виразу та збереження в історію
        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(expression)) return;

            var original = expression;
            try
            {
                // Замінюємо візуальні оператори на стандартні для DataTable
                var exp = expression.Replace("×", "*")
                                     .Replace("÷", "/")
                                     .Replace("−", "-");
                // Обробка відсотків: замінюємо '%' на '/100'
                var newExp = "";
                for (int j = 0; j < exp.Length; j++)
                {
                    if (exp[j] == '%')
                    {
                        newExp += "/100";
                        if (j + 1 < exp.Length && (char.IsDigit(exp[j + 1]) || exp[j + 1] == '('))
                            newExp += "*";
                    }
                    else newExp += exp[j];
                }

                // Виконуємо обчислення через DataTable
                var resultObj = new DataTable().Compute(newExp, null);
                double result = Convert.ToDouble(resultObj, CultureInfo.InvariantCulture);

                // Зберігаємо початковий вираз і результат
                SaveHistory(original, result);
                expression = result.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Невірний вираз!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                expression = "";
            }

            // Скидаємо стан дужок і оновлюємо дисплей
            isOpenParenthesis = true;
            UpdateDisplay();
        }

        // Показ/сховання панелі історії
        private void History_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryPanel.Visibility == Visibility.Collapsed)
            {
                LoadHistory();
                HistoryPanel.Visibility = Visibility.Visible;

                // Встановлюємо фокус на список і обираємо перший запис
                if (HistoryListBox.Items.Count > 0)
                {
                    HistoryListBox.SelectedIndex = 0;
                    HistoryListBox.Focus();
                    Keyboard.Focus(HistoryListBox);
                }
            }
            else
            {
                HistoryPanel.Visibility = Visibility.Collapsed;
            }
        }

        // Очищення історії в базі та UI
        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var sql = GetProviderConnectionString();
            using (var conn = new SqlConnection(sql))
            using (var cmd = new SqlCommand("DELETE FROM [dbo].[Історія]", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            HistoryListBox.Items.Clear();
        }

        // Завантаження записів історії з БД у ListBox
        private void LoadHistory()
        {
            HistoryListBox.Items.Clear();
            var sql = GetProviderConnectionString();
            using (var conn = new SqlConnection(sql))
            using (var cmd = new SqlCommand("SELECT [Приклад],[Результат],[Дата] FROM [dbo].[Історія] ORDER BY [Id]", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var example = reader.GetString(0);
                        var result = reader.GetString(1);
                        var date = reader.GetDateTime(2);
                        var text = $"{date:dd.MM.yyyy HH:mm:ss}  →  {example} = {result}";
                        HistoryListBox.Items.Add(text);
                    }
                }
            }
        }

        // Застосування вибраного запису історії в поле вводу
        private void ApplyHistorySelection()
        {
            if (HistoryListBox.SelectedItem is string text)
            {
                int arrow = text.IndexOf('→');
                int eq = text.LastIndexOf('=');
                if (arrow >= 0 && eq > arrow)
                {
                    var expr = text.Substring(arrow + 1, eq - arrow - 1).Trim();
                    expression = expr;

                    var openCount = expression.Count(c => c == '(');
                    var closeCount = expression.Count(c => c == ')');
                    isOpenParenthesis = openCount <= closeCount;

                    UpdateDisplay();
                    HistoryPanel.Visibility = Visibility.Collapsed;
                    HistoryListBox.SelectedItem = null;
                }
            }
        }

        // Обробка клавіш у списку історії: Space/Enter для вибору
        private void HistoryListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                ApplyHistorySelection();
                e.Handled = true;
            }
        }

        // Обробник кліку миші: один клік для вибору запису
        private void HistoryListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ApplyHistorySelection();
        }

        // Збереження запису історії в базу даних
        private void SaveHistory(string example, double result)
        {
            var sql = GetProviderConnectionString();
            using (var conn = new SqlConnection(sql))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Генеруємо новий ID
                        var cmdId = new SqlCommand("SELECT ISNULL(MAX([Id]),0)+1 FROM [dbo].[Історія]", conn, tran);
                        var newId = (int)cmdId.ExecuteScalar();

                        // Вставка нового запису
                        var cmdIns = new SqlCommand(
                            "INSERT INTO [dbo].[Історія]([Id],[Приклад],[Результат],[Дата]) VALUES(@Id,@ex,@res,@dt)",
                            conn, tran);
                        cmdIns.Parameters.AddWithValue("@Id", newId);
                        cmdIns.Parameters.AddWithValue("@ex", example);
                        cmdIns.Parameters.AddWithValue("@res", result.ToString(CultureInfo.InvariantCulture));
                        cmdIns.Parameters.AddWithValue("@dt", DateTime.Now);
                        cmdIns.ExecuteNonQuery();

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        // Отримання чистого рядка підключення до SQL Server з налаштувань Entity Framework
        private string GetProviderConnectionString()
        {
            var efcs = ConfigurationManager.ConnectionStrings["CalculatorEntities"].ConnectionString;
            var builder = new EntityConnectionStringBuilder(efcs);
            return builder.ProviderConnectionString;
        }

        // --- Обробники клавіатури головного вікна ---
        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var text = e.Text;
            if (int.TryParse(text, out _))
            {
                expression += text;
                UpdateDisplay();
            }
            else
            {
                switch (text)
                {
                    case ".": Decimal_Click(null, null); break;
                    case "+": Operator_Click(new Button { Content = "+" }, null); break;
                    case "-": Operator_Click(new Button { Content = "−" }, null); break;
                    case "*": Operator_Click(new Button { Content = "×" }, null); break;
                    case "/": Operator_Click(new Button { Content = "÷" }, null); break;
                    case "%": Percent_Click(null, null); break;
                    case "(":
                    case ")":
                        expression += text;
                        UpdateDisplay();
                        break;
                }
            }
            e.Handled = true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                History_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.Delete)
            {
                ClearHistory_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.Back)
            {
                Backspace_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Clear_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Equals_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                var num = (e.Key - Key.NumPad0).ToString();
                expression += num;
                UpdateDisplay();
                e.Handled = true;
            }
            else if (e.Key == Key.Decimal)
            {
                Decimal_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Add)
            {
                Operator_Click(new Button { Content = "+" }, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract)
            {
                Operator_Click(new Button { Content = "−" }, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Multiply)
            {
                Operator_Click(new Button { Content = "×" }, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Divide)
            {
                Operator_Click(new Button { Content = "÷" }, null);
                e.Handled = true;
            }
            else if (e.Key == Key.P)
            {
                Negate_Click(null, null);
                e.Handled = true;
            }
        }
    }
}
