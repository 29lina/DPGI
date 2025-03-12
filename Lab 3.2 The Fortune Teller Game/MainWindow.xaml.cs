using System;
using System.Windows;
using System.Windows.Threading;

namespace Voroška
{
    public partial class MainWindow : Window
    {
        private readonly string[] answers = { "Так", "Ні", "Скоріше так", "Скоріше ні" };
        private static readonly Random rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть питання.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int index = rnd.Next(answers.Length);
            AnswerTextBlock.Text = answers[index];
        }

        private void QuestionTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
