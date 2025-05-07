using System.Windows;
using System.Windows.Controls;

namespace WpfApplProject
{
    public partial class PageMain : Page
    {
        public PageMain()
        {
            InitializeComponent();
        }

        private void Calculator_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageCalculator());
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageHistory());
        }
    }
}
