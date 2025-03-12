using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CalculatorWPF
{
    public partial class MainWindow : Window
    {
        // Змінні для збереження попереднього числа, результату операції та вибраного оператора
        private double lastNumber, result;
        private string selectedOperator;
        // Прапорець для сигналізації, що після натискання оператора дисплей потрібно очистити
        private bool isOperatorClicked;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Обробник натискання кнопок із цифрами
        private void Number_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string number = button.Content.ToString();

            if (Display.Text == "0" || isOperatorClicked)
            {
                Display.Text = number;
                isOperatorClicked = false;
            }
            else
            {
                Display.Text += number;
            }
        }

        // Обробник натискання кнопки десяткової крапки
        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            if (!Display.Text.Contains("."))
                Display.Text += ".";
        }

        // Обробник кнопки очищення (C)
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Display.Text = "0";
            lastNumber = 0;
            result = 0;
            selectedOperator = string.Empty;
        }

        // Обробник кнопок-операторів (+, −, ×, ÷)
        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string op = button.Content.ToString();

            // Зберігаємо перше число та вибраний оператор
            if (double.TryParse(Display.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out lastNumber))
            {
                selectedOperator = op;
                isOperatorClicked = true;
            }
        }

        // Обчислення результату при натисканні "="
        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(Display.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double newNumber))
                return;

            switch (selectedOperator)
            {
                case "+":
                    result = lastNumber + newNumber;
                    break;
                case "−":
                    result = lastNumber - newNumber;
                    break;
                case "×":
                    result = lastNumber * newNumber;
                    break;
                case "÷":
                    if (newNumber == 0)
                    {
                        MessageBox.Show("Ділення на нуль неможливе!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    result = lastNumber / newNumber;
                    break;
                default:
                    result = newNumber;
                    break;
            }
            Display.Text = result.ToString(CultureInfo.InvariantCulture);
            isOperatorClicked = true;
        }

        // Обробник для зміни знаку (±)
        private void Negate_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Display.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                number = -number;
                Display.Text = number.ToString(CultureInfo.InvariantCulture);
            }
        }

        // Обробник для перетворення числа в відсотки (%)
        private void Percent_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Display.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                number = number / 100;
                Display.Text = number.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
