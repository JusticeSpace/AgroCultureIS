using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Diagnostics;

// Excel
using ClosedXML.Excel;

// PDF - используем полные имена для избежания конфликтов
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace AgroCulture.ViewModels
{
    public class BookingsListViewModel : BaseViewModel
    {
        // ═══════════════════════════════════════════════════════════
        // СВОЙСТВА ДАННЫХ
        // ═══════════════════════════════════════════════════════════

        private ObservableCollection<BookingsDetails> _allBookings;
        public ObservableCollection<BookingsDetails> AllBookings
        {
            get => _allBookings;
            set => SetProperty(ref _allBookings, value);
        }

        private ObservableCollection<BookingsDetails> _filteredBookings;
        public ObservableCollection<BookingsDetails> FilteredBookings
        {
            get => _filteredBookings;
            set => SetProperty(ref _filteredBookings, value);
        }

        // ═══════════════════════════════════════════════════════════
        // СТАТИСТИКА
        // ═══════════════════════════════════════════════════════════

        private int _totalBookings;
        public int TotalBookings
        {
            get => _totalBookings;
            set => SetProperty(ref _totalBookings, value);
        }

        private int _activeBookings;
        public int ActiveBookings
        {
            get => _activeBookings;
            set => SetProperty(ref _activeBookings, value);
        }

        private int _completedBookings;
        public int CompletedBookings
        {
            get => _completedBookings;
            set => SetProperty(ref _completedBookings, value);
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        private int _filteredCount;
        public int FilteredCount
        {
            get => _filteredCount;
            set => SetProperty(ref _filteredCount, value);
        }

        // ═══════════════════════════════════════════════════════════
        // ФИЛЬТРЫ
        // ═══════════════════════════════════════════════════════════

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ApplyFilters();
                }
            }
        }

        private string _statusFilter = "all";
        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (SetProperty(ref _statusFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ПРАВА ДОСТУПА
        // ═══════════════════════════════════════════════════════════

        private bool _showActionsColumn;
        public bool ShowActionsColumn
        {
            get => _showActionsColumn;
            set => SetProperty(ref _showActionsColumn, value);
        }

        // ═══════════════════════════════════════════════════════════
        // КОМАНДЫ
        // ═══════════════════════════════════════════════════════════

        public RelayCommand<string> SetStatusFilterCommand { get; }
        public RelayCommand<BookingsDetails> EditBookingCommand { get; }
        public RelayCommand<BookingsDetails> DeleteBookingCommand { get; }
        public RelayCommand ExportToExcelCommand { get; }
        public RelayCommand ExportToPdfCommand { get; }

        public event Action<string, bool> ShowNotification;

        // ═══════════════════════════════════════════════════════════
        // КОНСТРУКТОР
        // ═══════════════════════════════════════════════════════════

        public BookingsListViewModel()
        {
            AllBookings = new ObservableCollection<BookingsDetails>();
            FilteredBookings = new ObservableCollection<BookingsDetails>();

            SetStatusFilterCommand = new RelayCommand<string>(SetStatusFilter);
            EditBookingCommand = new RelayCommand<BookingsDetails>(EditBooking);
            DeleteBookingCommand = new RelayCommand<BookingsDetails>(DeleteBooking);
            ExportToExcelCommand = new RelayCommand(_ => ExportToExcel());
            ExportToPdfCommand = new RelayCommand(_ => ExportToPdf());

            CheckPermissions();
            LoadBookings();
        }

        // ═══════════════════════════════════════════════════════════
        // ПРОВЕРКА ПРАВ ДОСТУПА
        // ═══════════════════════════════════════════════════════════

        private void CheckPermissions()
        {
            if (App.CurrentUser != null)
            {
                var role = App.CurrentUser.Role?.ToLower();
                ShowActionsColumn = role == "admin" || role == "manager";
            }
            else
            {
                ShowActionsColumn = false;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ЗАГРУЗКА ДАННЫХ
        // ═══════════════════════════════════════════════════════════

        public void LoadBookings()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var bookings = context.BookingsDetails
                        .OrderByDescending(b => b.BookingId)
                        .ToList();

                    AllBookings.Clear();
                    foreach (var booking in bookings)
                    {
                        AllBookings.Add(booking);
                    }

                    CalculateStatistics();
                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки бронирований:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // РАСЧЕТ СТАТИСТИКИ
        // ═══════════════════════════════════════════════════════════

        private void CalculateStatistics()
        {
            TotalBookings = AllBookings.Count;
            ActiveBookings = AllBookings.Count(b => b.Status == "active");
            CompletedBookings = AllBookings.Count(b => b.Status == "completed");
            TotalRevenue = AllBookings.Sum(b => b.TotalPrice);
        }

        // ═══════════════════════════════════════════════════════════
        // ФИЛЬТРАЦИЯ
        // ═══════════════════════════════════════════════════════════

        private void SetStatusFilter(string status)
        {
            StatusFilter = status;
        }

        private void ApplyFilters()
        {
            var filtered = AllBookings.AsEnumerable();

            if (StatusFilter != "all")
            {
                filtered = filtered.Where(b => b.Status == StatusFilter);
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.Trim().ToLower();

                filtered = filtered.Where(b =>
                    (b.GuestName != null && b.GuestName.ToLower().Contains(query)) ||
                    (b.GuestPhone != null && b.GuestPhone.Contains(query)) ||
                    (b.CabinName != null && b.CabinName.ToLower().Contains(query))
                );
            }

            FilteredBookings.Clear();
            foreach (var booking in filtered)
            {
                FilteredBookings.Add(booking);
            }

            FilteredCount = FilteredBookings.Count;
        }

        // ═══════════════════════════════════════════════════════════
        // РЕДАКТИРОВАНИЕ БРОНИРОВАНИЯ
        // ═══════════════════════════════════════════════════════════

        private void EditBooking(BookingsDetails bookingDetails)
        {
            if (bookingDetails == null) return;

            try
            {
                var window = new Views.BookingEditWindow(bookingDetails.BookingId);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();

                if (window.DialogResultSuccess)
                {
                    // Перезагрузка данных
                    LoadBookings();

                    ShowNotification?.Invoke("✅ Бронирование успешно обновлено", true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ЭКСПОРТ В EXCEL
        // ═══════════════════════════════════════════════════════════

        private void ExportToExcel()
        {
            try
            {
                if (FilteredBookings == null || FilteredBookings.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Бронирования_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx",
                    DefaultExt = ".xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Бронирования");

                        // Заголовок
                        worksheet.Cell(1, 1).Value = "Отчет по бронированиям";
                        worksheet.Range(1, 1, 1, 9).Merge();
                        worksheet.Cell(1, 1).Style
                            .Font.SetBold()
                            .Font.SetFontSize(16)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        worksheet.Cell(2, 1).Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
                        worksheet.Range(2, 1, 2, 9).Merge();
                        worksheet.Cell(2, 1).Style
                            .Font.SetFontSize(10)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        // Заголовки таблицы
                        int headerRow = 4;
                        worksheet.Cell(headerRow, 1).Value = "№";
                        worksheet.Cell(headerRow, 2).Value = "Домик";
                        worksheet.Cell(headerRow, 3).Value = "Гость";
                        worksheet.Cell(headerRow, 4).Value = "Телефон";
                        worksheet.Cell(headerRow, 5).Value = "Заезд";
                        worksheet.Cell(headerRow, 6).Value = "Выезд";
                        worksheet.Cell(headerRow, 7).Value = "Ночей";
                        worksheet.Cell(headerRow, 8).Value = "Сумма (₽)";
                        worksheet.Cell(headerRow, 9).Value = "Статус";

                        var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
                        headerRange.Style
                            .Font.SetBold()
                            .Font.SetFontColor(XLColor.White)
                            .Fill.SetBackgroundColor(XLColor.FromHtml("#15803D"))
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                            .Border.SetOutsideBorder(XLBorderStyleValues.Medium);

                        // Данные
                        int currentRow = headerRow + 1;
                        foreach (var booking in FilteredBookings)
                        {
                            worksheet.Cell(currentRow, 1).Value = booking.BookingId;
                            worksheet.Cell(currentRow, 2).Value = booking.CabinName ?? "";
                            worksheet.Cell(currentRow, 3).Value = booking.GuestName ?? "";
                            worksheet.Cell(currentRow, 4).Value = booking.GuestPhone ?? "";
                            worksheet.Cell(currentRow, 5).Value = booking.CheckInDate.ToString("dd.MM.yyyy");
                            worksheet.Cell(currentRow, 6).Value = booking.CheckOutDate.ToString("dd.MM.yyyy");
                            worksheet.Cell(currentRow, 7).Value = booking.Nights;
                            worksheet.Cell(currentRow, 8).Value = booking.TotalPrice;
                            worksheet.Cell(currentRow, 9).Value = booking.Status == "active" ? "Активно" : "Завершено";

                            if (currentRow % 2 == 0)
                            {
                                worksheet.Range(currentRow, 1, currentRow, 9)
                                    .Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F9FAFB"));
                            }

                            currentRow++;
                        }

                        // Итого
                        int totalRow = currentRow + 1;
                        worksheet.Cell(totalRow, 1).Value = "ИТОГО:";
                        worksheet.Range(totalRow, 1, totalRow, 7).Merge();
                        worksheet.Cell(totalRow, 1).Style
                            .Font.SetBold()
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                        worksheet.Cell(totalRow, 8).Value = FilteredBookings.Sum(b => b.TotalPrice);
                        worksheet.Cell(totalRow, 8).Style
                            .Font.SetBold()
                            .Font.SetFontColor(XLColor.FromHtml("#15803D"))
                            .NumberFormat.Format = "#,##0.00 ₽";

                        // Форматирование
                        worksheet.Columns().AdjustToContents();
                        worksheet.Column(1).Width = 8;
                        worksheet.Column(7).Width = 10;
                        worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00 ₽";
                        worksheet.Column(8).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        worksheet.Column(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Column(7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Column(9).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        var tableRange = worksheet.Range(headerRow, 1, currentRow - 1, 9);
                        tableRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                        tableRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    ShowNotification?.Invoke(
                        $"✅ Успешно экспортировано {FilteredCount} записей в Excel\n" +
                        $"Файл: {System.IO.Path.GetFileName(saveFileDialog.FileName)}",
                        true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка экспорта в Excel:\n\n{ex.Message}\n\n" +
                    $"Убедитесь, что:\n" +
                    $"• Файл не открыт в другой программе\n" +
                    $"• Установлен пакет ClosedXML",
                    "Ошибка экспорта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ЭКСПОРТ В PDF
        // ═══════════════════════════════════════════════════════════

        private void ExportToPdf()
        {
            try
            {
                if (FilteredBookings == null || FilteredBookings.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    FileName = $"Бронирования_{DateTime.Now:yyyy-MM-dd_HH-mm}.pdf",
                    DefaultExt = ".pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Создание документа
                    Document document = new Document();
                    document.Info.Title = "Отчет по бронированиям";
                    document.Info.Subject = "AgroCulture - База отдыха Лесная усадьба";
                    document.Info.Author = "AgroCulture System";

                    // Стили (используем полное имя для избежания конфликта)
                    MigraDoc.DocumentObjectModel.Style normalStyle = document.Styles["Normal"];
                    normalStyle.Font.Name = "Arial";
                    normalStyle.Font.Size = 10;

                    MigraDoc.DocumentObjectModel.Style heading1Style = document.Styles["Heading1"];
                    heading1Style.Font.Size = 18;
                    heading1Style.Font.Bold = true;
                    heading1Style.Font.Color = Colors.DarkGreen;

                    // Секция
                    Section section = document.AddSection();
                    section.PageSetup.PageFormat = PageFormat.A4;
                    section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;
                    section.PageSetup.LeftMargin = "2cm";
                    section.PageSetup.RightMargin = "2cm";
                    section.PageSetup.TopMargin = "2cm";
                    section.PageSetup.BottomMargin = "2cm";

                    // Заголовок
                    Paragraph title = section.AddParagraph("Отчет по бронированиям");
                    title.Format.Font.Size = 18;
                    title.Format.Font.Bold = true;
                    title.Format.Font.Color = Colors.DarkGreen;
                    title.Format.Alignment = ParagraphAlignment.Center;
                    title.Format.SpaceAfter = "0.5cm";

                    Paragraph subtitle = section.AddParagraph(
                        $"База отдыха «Лесная усадьба»\n" +
                        $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    subtitle.Format.Font.Size = 10;
                    subtitle.Format.Alignment = ParagraphAlignment.Center;
                    subtitle.Format.SpaceAfter = "1cm";

                    // Статистика
                    Table statsTable = section.AddTable();
                    statsTable.Borders.Width = 0.75;
                    statsTable.Borders.Color = Colors.Gray;

                    statsTable.AddColumn("4cm");
                    statsTable.AddColumn("4cm");
                    statsTable.AddColumn("4cm");
                    statsTable.AddColumn("4cm");

                    Row statsRow = statsTable.AddRow();
                    statsRow.Shading.Color = Colors.LightGreen;
                    statsRow.HeadingFormat = true;
                    statsRow.Format.Font.Bold = true;

                    statsRow.Cells[0].AddParagraph($"Всего: {TotalBookings}");
                    statsRow.Cells[1].AddParagraph($"Активных: {ActiveBookings}");
                    statsRow.Cells[2].AddParagraph($"Завершенных: {CompletedBookings}");
                    statsRow.Cells[3].AddParagraph($"Выручка: {TotalRevenue:N0} ₽");

                    // ✅ ИСПРАВЛЕНИЕ: используем полное имя для VerticalAlignment
                    foreach (Cell cell in statsRow.Cells)
                    {
                        cell.Format.Alignment = ParagraphAlignment.Center;
                        cell.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                    }

                    section.AddParagraph().Format.SpaceAfter = "1cm";

                    // Таблица бронирований
                    Table table = section.AddTable();
                    table.Borders.Width = 0.75;
                    table.Borders.Color = Colors.Gray;

                    table.AddColumn("1.5cm");
                    table.AddColumn("3cm");
                    table.AddColumn("3.5cm");
                    table.AddColumn("2.5cm");
                    table.AddColumn("2cm");
                    table.AddColumn("2cm");
                    table.AddColumn("1.5cm");
                    table.AddColumn("2.5cm");
                    table.AddColumn("2cm");

                    // Заголовок таблицы
                    Row headerRow = table.AddRow();
                    headerRow.Shading.Color = new Color(21, 128, 61);
                    headerRow.HeadingFormat = true;
                    headerRow.Format.Font.Bold = true;
                    headerRow.Format.Font.Color = Colors.White;
                    headerRow.Format.Alignment = ParagraphAlignment.Center;
                    // ✅ ИСПРАВЛЕНИЕ: полное имя
                    headerRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

                    headerRow.Cells[0].AddParagraph("№");
                    headerRow.Cells[1].AddParagraph("Домик");
                    headerRow.Cells[2].AddParagraph("Гость");
                    headerRow.Cells[3].AddParagraph("Телефон");
                    headerRow.Cells[4].AddParagraph("Заезд");
                    headerRow.Cells[5].AddParagraph("Выезд");
                    headerRow.Cells[6].AddParagraph("Ночей");
                    headerRow.Cells[7].AddParagraph("Сумма");
                    headerRow.Cells[8].AddParagraph("Статус");

                    // Данные
                    bool isAlternate = false;
                    foreach (var booking in FilteredBookings)
                    {
                        Row row = table.AddRow();

                        if (isAlternate)
                        {
                            row.Shading.Color = new Color(249, 250, 251);
                        }
                        isAlternate = !isAlternate;

                        row.Cells[0].AddParagraph(booking.BookingId.ToString());
                        row.Cells[0].Format.Alignment = ParagraphAlignment.Center;

                        row.Cells[1].AddParagraph(booking.CabinName ?? "");
                        row.Cells[1].Format.Font.Bold = true;
                        row.Cells[1].Format.Font.Color = new Color(21, 128, 61);

                        row.Cells[2].AddParagraph(booking.GuestName ?? "");
                        row.Cells[3].AddParagraph(booking.GuestPhone ?? "");
                        row.Cells[3].Format.Font.Size = 9;

                        row.Cells[4].AddParagraph(booking.CheckInDate.ToString("dd.MM.yyyy"));
                        row.Cells[4].Format.Alignment = ParagraphAlignment.Center;

                        row.Cells[5].AddParagraph(booking.CheckOutDate.ToString("dd.MM.yyyy"));
                        row.Cells[5].Format.Alignment = ParagraphAlignment.Center;

                        row.Cells[6].AddParagraph(booking.Nights.ToString());
                        row.Cells[6].Format.Alignment = ParagraphAlignment.Center;

                        row.Cells[7].AddParagraph($"{booking.TotalPrice:N0} ₽");
                        row.Cells[7].Format.Alignment = ParagraphAlignment.Right;
                        row.Cells[7].Format.Font.Bold = true;

                        string statusText = booking.Status == "active" ? "Активно" : "Завершено";
                        row.Cells[8].AddParagraph(statusText);
                        row.Cells[8].Format.Alignment = ParagraphAlignment.Center;
                        row.Cells[8].Format.Font.Size = 9;
                    }

                    // Итого
                    Row totalRow = table.AddRow();
                    totalRow.Shading.Color = Colors.LightGray;
                    totalRow.Format.Font.Bold = true;

                    totalRow.Cells[0].MergeRight = 6;
                    totalRow.Cells[0].AddParagraph("ИТОГО:");
                    totalRow.Cells[0].Format.Alignment = ParagraphAlignment.Right;

                    totalRow.Cells[7].AddParagraph($"{FilteredBookings.Sum(b => b.TotalPrice):N0} ₽");
                    totalRow.Cells[7].Format.Alignment = ParagraphAlignment.Right;
                    totalRow.Cells[7].Format.Font.Color = new Color(21, 128, 61);

                    // Футер
                    Paragraph footer = section.Footers.Primary.AddParagraph();
                    footer.AddText($"Страница ");
                    footer.AddPageField();
                    footer.AddText(" из ");
                    footer.AddNumPagesField();
                    footer.Format.Alignment = ParagraphAlignment.Center;
                    footer.Format.Font.Size = 9;

                    // Рендеринг
                    PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
                    pdfRenderer.Document = document;
                    pdfRenderer.RenderDocument();
                    pdfRenderer.PdfDocument.Save(saveFileDialog.FileName);

                    ShowNotification?.Invoke(
                        $"✅ Успешно экспортировано {FilteredCount} записей в PDF\n" +
                        $"Файл: {System.IO.Path.GetFileName(saveFileDialog.FileName)}",
                        true);

                    var result = MessageBox.Show(
                        "PDF файл успешно создан!\n\nОткрыть файл?",
                        "Экспорт завершен",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка экспорта в PDF:\n\n{ex.Message}\n\n" +
                    $"Убедитесь, что:\n" +
                    $"• Файл не открыт в другой программе\n" +
                    $"• Установлены пакеты PdfSharp и MigraDoc",
                    "Ошибка экспорта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        // ═══════════════════════════════════════════════════════════
        // ✅ НОВОЕ: УДАЛЕНИЕ БРОНИРОВАНИЯ
        // ═══════════════════════════════════════════════════════════

        private void DeleteBooking(BookingsDetails bookingDetails)
        {
            if (bookingDetails == null) return;

            // Подтверждение удаления
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить бронирование?\n\n" +
                $"Домик: {bookingDetails.CabinName}\n" +
                $"Гость: {bookingDetails.GuestName}\n" +
                $"Даты: {bookingDetails.CheckInDate:dd.MM.yyyy} - {bookingDetails.CheckOutDate:dd.MM.yyyy}\n" +
                $"Сумма: {bookingDetails.TotalPrice:N0} ₽\n\n" +
                $"Это действие нельзя отменить!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // Находим бронирование в БД
                    var booking = context.Bookings
                        .FirstOrDefault(b => b.BookingId == bookingDetails.BookingId);

                    if (booking == null)
                    {
                        MessageBox.Show("Бронирование не найдено в базе данных", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    booking.Status = "cancelled";

                    context.SaveChanges();

                    // Обновление UI
                    LoadBookings(); // Перезагружаем список

                    ShowNotification?.Invoke(
                        $"✅ Бронирование #{bookingDetails.BookingId} успешно удалено",
                        true);

                    MessageBox.Show(
                        "Бронирование успешно удалено!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при удалении бронирования:\n\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}