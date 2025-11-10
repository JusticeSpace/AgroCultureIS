using System;
using System.Windows;
using System.Windows.Controls;
using AgroCulture.ViewModels;
using MaterialDesignThemes.Wpf;

namespace AgroCulture.Views
{
    public partial class CabinBookingView : UserControl
    {
        private CabinBookingViewModel ViewModel => DataContext as CabinBookingViewModel;

        public CabinBookingView()
        {
            InitializeComponent();

            Loaded += CabinBookingView_Loaded;
            Unloaded += CabinBookingView_Unloaded;
        }

        // ✅ НОВОЕ: Подписка при загрузке
        private void CabinBookingView_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[CABIN VIEW] Loaded");

            if (ViewModel != null)
            {
                // Отписываемся (на случай повторной загрузки)
                ViewModel.ShowNotification -= ShowNotification;

                // Подписываемся
                ViewModel.ShowNotification += ShowNotification;
            }
        }

        // ✅ НОВОЕ: Отписка при выгрузке
        private void CabinBookingView_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[CABIN VIEW] Unloaded - отписка от событий");

            if (ViewModel != null)
            {
                ViewModel.ShowNotification -= ShowNotification;
            }
        }

        private void ShowNotification(string message, bool isSuccess)
        {
            try
            {
                var messageQueue = BookingSnackbar.MessageQueue ?? new SnackbarMessageQueue();

                if (isSuccess)
                {
                    messageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(4));
                }
                else
                {
                    messageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(3));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CABIN VIEW] Ошибка Snackbar: {ex.Message}");
            }
        }
    }
}