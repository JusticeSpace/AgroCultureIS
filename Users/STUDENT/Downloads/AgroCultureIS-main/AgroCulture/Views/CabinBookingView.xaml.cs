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

        // ✅ НОВОЕ СВОЙСТВО
        public bool IsGuestMode { get; private set; }

        // ✅ КОНСТРУКТОР ПО УМОЛЧАНИЮ
        public CabinBookingView() : this(false)
        {
        }

        // ✅ КОНСТРУКТОР С ПАРАМЕТРОМ (для гостевого режима)
        public CabinBookingView(bool isGuestMode)
        {
            InitializeComponent();
            IsGuestMode = isGuestMode;

            Loaded += CabinBookingView_Loaded;
        }

        private void CabinBookingView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ShowNotification += ShowNotification;

                // ✅ ЕСЛИ ГОСТЕВОЙ РЕЖИМ - СКРЫВАЕМ ФОРМУ
                if (IsGuestMode)
                {
                    ConfigureGuestMode();
                }
            }
        }

        /// <summary>
        /// Настройка гостевого режима (только просмотр каталога)
        /// </summary>
        private void ConfigureGuestMode()
        {
            // ✅ СКРЫВАЕМ ПРАВУЮ КОЛОНКУ (ФОРМУ БРОНИРОВАНИЯ)
            // Предполагаем, что Grid.Column="2" - это форма
            if (this.Content is Grid mainGrid && mainGrid.ColumnDefinitions.Count >= 3)
            {
                // Находим правую колонку
                var formColumn = mainGrid.ColumnDefinitions[2];
                formColumn.Width = new GridLength(0); // Скрываем

                // Убираем разделитель
                var spacerColumn = mainGrid.ColumnDefinitions[1];
                spacerColumn.Width = new GridLength(0);

                // Растягиваем каталог на всю ширину
                var catalogColumn = mainGrid.ColumnDefinitions[0];
                catalogColumn.Width = new GridLength(1, GridUnitType.Star);
            }

            // ✅ ПОКАЗЫВАЕМ ИНФОРМАЦИОННЫЙ БАННЕР
            ShowGuestModeBanner();
        }

        /// <summary>
        /// Показать баннер гостевого режима
        /// </summary>
        private void ShowGuestModeBanner()
        {
            var messageQueue = BookingSnackbar.MessageQueue ?? new SnackbarMessageQueue();
            messageQueue.Enqueue(
                "Гостевой режим: Вы можете просматривать доступные домики. Для бронирования обратитесь к администратору.",
                "ПОНЯТНО",
                (param) => { },
                null,
                false,
                true,
                TimeSpan.FromSeconds(6)
            );
        }

        private void ShowNotification(string message, bool isSuccess)
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
    }
}