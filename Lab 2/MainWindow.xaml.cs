using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Lab_2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CommandBinding saveCommand = new CommandBinding(ApplicationCommands.Save, execute_Save, canExecute_Save);
            CommandBinding openCommand = new CommandBinding(ApplicationCommands.Open, execute_Open, canExecute_Open);
            CommandBinding clearCommand = new CommandBinding(ApplicationCommands.Cut, execute_Clear, canExecute_Clear);

            CommandBindings.Add(saveCommand);
            CommandBindings.Add(openCommand);
            CommandBindings.Add(clearCommand);
        }

        void canExecute_Save(object sender, CanExecuteRoutedEventArgs e)
        {
            if (inputTextBox.Text.Trim().Length > 0)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        void execute_Save(object sender, ExecutedRoutedEventArgs e)
        {
            System.IO.File.WriteAllText("C:\\Users\\taran\\OneDrive\\Робочий стіл\\Графічні інтерфейси\\Лабораторна робота №2\\File.txt", inputTextBox.Text);
            MessageBox.Show("Файл збережено!");
        }

        void canExecute_Open(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = inputTextBox != null;
        }

        void execute_Open(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Виберіть файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = System.IO.File.ReadAllText(openFileDialog.FileName);
                    inputTextBox.Text = fileContent;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка відкриття файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void canExecute_Clear(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = inputTextBox != null && !string.IsNullOrWhiteSpace(inputTextBox.Text);
        }

        void execute_Clear(object sender, ExecutedRoutedEventArgs e)
        {
            inputTextBox.Clear();
        }
    }
}
