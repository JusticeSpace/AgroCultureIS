using System;
using System.Windows;
using System.Windows.Controls;
using AgroCulture.ViewModels;
using MaterialDesignThemes.Wpf;

namespace AgroCulture.Views
{
    public partial class CabinsManagementView : UserControl
    {
        private CabinsManagementViewModel ViewModel => DataContext as CabinsManagementViewModel;

        public CabinsManagementView()
        {
            InitializeComponent();
            Loaded += CabinsManagementView_Loaded;
        }

        private void CabinsManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ShowNotification -= ShowNotification;
                ViewModel.ShowNotification += ShowNotification;
                ViewModel.RefreshData();
            }
        }

        private void ShowNotification(string message, bool isSuccess)
        {
            try
            {
                var messageQueue = NotificationSnackbar.MessageQueue ?? new SnackbarMessageQueue();
                NotificationSnackbar.MessageQueue = messageQueue;
                messageQueue.Enqueue(message);
            }
            catch { }
        }
    }
}