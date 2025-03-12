using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TimerApp
{
    public partial class MainWindow : Window
    {
        public static readonly RoutedCommand StartTimerCommand = new RoutedCommand();
        public static readonly RoutedCommand ResetTimerCommand = new RoutedCommand();

        private DispatcherTimer timer;
        private TimeSpan timeLeft;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;

            CommandBindings.Add(new CommandBinding(StartTimerCommand, ExecuteStartTimer, CanExecuteStartTimer));
            CommandBindings.Add(new CommandBinding(ResetTimerCommand, ExecuteResetTimer, CanExecuteResetTimer));
        }

        private void CanExecuteStartTimer(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !timer.IsEnabled;
        }

        private void ExecuteStartTimer(object sender, ExecutedRoutedEventArgs e)
        {
            if (double.TryParse(MinutesTextBox.Text, out double minutes) && minutes > 0)
            {
                timeLeft = TimeSpan.FromMinutes(minutes);
                CountdownTextBlock.Text = timeLeft.ToString(@"hh\:mm\:ss");

                timer.Start();
                MinutesTextBox.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Введіть коректне число хвилин.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timeLeft.TotalSeconds > 1)
            {
                timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
                CountdownTextBlock.Text = timeLeft.ToString(@"hh\:mm\:ss");
            }
            else
            {
                timer.Stop();
                CountdownTextBlock.Text = "00:00:00";
                MessageBox.Show("Час вичерпано!", "Таймер", MessageBoxButton.OK, MessageBoxImage.Information);
                MinutesTextBox.IsEnabled = true;
            }
        }

        private void CanExecuteResetTimer(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = timer.IsEnabled || CountdownTextBlock.Text != "00:00:00";
        }

        private void ExecuteResetTimer(object sender, ExecutedRoutedEventArgs e)
        {
            timer.Stop();
            CountdownTextBlock.Text = "00:00:00";
            MinutesTextBox.IsEnabled = true;
        }
    }
}
