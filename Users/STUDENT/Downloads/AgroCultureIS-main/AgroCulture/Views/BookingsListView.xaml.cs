using System.Windows;
using System.Windows.Controls;
using AgroCulture.ViewModels;

namespace AgroCulture.Views
{
    public partial class BookingsListView : UserControl
    {
        private BookingsListViewModel ViewModel => DataContext as BookingsListViewModel;

        public BookingsListView()
        {
            InitializeComponent();

            Loaded += BookingsListView_Loaded;
        }

        private void BookingsListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                // Подписка на события
                ViewModel.ShowNotification += ShowNotification;

                // ✅ Перезагрузка данных при каждом открытии
                ViewModel.LoadBookings();
            }
        }

        private void ShowNotification(string message, bool isSuccess)
        {
            MessageBox.Show(message, isSuccess ? "Успех" : "Ошибка",
                MessageBoxButton.OK,
                isSuccess ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
    }
}