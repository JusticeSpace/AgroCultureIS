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

            Loaded += (s, e) =>
            {
                if (ViewModel != null)
                {
                    ViewModel.ShowNotification += ShowNotification;
                }
            };
        }

        private void ShowNotification(string message, bool isSuccess)
        {
            var messageQueue = BookingSnackbar.MessageQueue ?? new SnackbarMessageQueue();

            // Устанавливаем цвет фона в зависимости от типа сообщения
            if (isSuccess)
            {
                messageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(4));
            }
            else
            {
                messageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(3));
            }
        }
    }
}