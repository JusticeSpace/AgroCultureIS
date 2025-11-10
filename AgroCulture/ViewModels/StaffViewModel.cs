using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AgroCulture.ViewModels;

namespace AgroCulture.ViewModels
{
    public class StaffViewModel : BaseViewModel
    {
        // ════════════════════════════════════════════════════════════
        // SINGLETON ПАТТЕРН
        // ════════════════════════════════════════════════════════════

        private static StaffViewModel _instance;
        public static StaffViewModel Instance => _instance ?? (_instance = new StaffViewModel());

        // ════════════════════════════════════════════════════════════
        // СВОЙСТВА ДЛЯ ДОБАВЛЕНИЯ НОВОГО СОТРУДНИКА
        // ════════════════════════════════════════════════════════════

        private string _selectedRole = "manager";
        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }

        private string _newLogin;
        public string NewLogin
        {
            get => _newLogin;
            set { _newLogin = value; OnPropertyChanged(); }
        }

        // ✅ НОВЫЕ: Отдельные поля для ФИО
        private string _newSurname;
        public string NewSurname
        {
            get => _newSurname;
            set { _newSurname = value; OnPropertyChanged(); }
        }

        private string _newFirstName;
        public string NewFirstName
        {
            get => _newFirstName;
            set { _newFirstName = value; OnPropertyChanged(); }
        }

        private string _newMiddleName;
        public string NewMiddleName
        {
            get => _newMiddleName;
            set { _newMiddleName = value; OnPropertyChanged(); }
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        // ════════════════════════════════════════════════════════════
        // КОЛЛЕКЦИИ И СТАТИСТИКА
        // ════════════════════════════════════════════════════════════

        private ObservableCollection<Users> _staffList;
        public ObservableCollection<Users> StaffList
        {
            get => _staffList;
            set { _staffList = value; OnPropertyChanged(); }
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set { _totalCount = value; OnPropertyChanged(); }
        }

        private int _adminCount;
        public int AdminCount
        {
            get => _adminCount;
            set { _adminCount = value; OnPropertyChanged(); }
        }

        private int _managerCount;
        public int ManagerCount
        {
            get => _managerCount;
            set { _managerCount = value; OnPropertyChanged(); }
        }

        // ════════════════════════════════════════════════════════════
        // КОМАНДЫ
        // ════════════════════════════════════════════════════════════

        public ICommand AddStaffCommand { get; }
        public ICommand EditStaffCommand { get; }
        public ICommand DeleteStaffCommand { get; }

        // ════════════════════════════════════════════════════════════
        // СОБЫТИЯ
        // ════════════════════════════════════════════════════════════

        public event Action<string, bool> ShowNotification;
        public event Action<Users> RequestEdit;

        // ════════════════════════════════════════════════════════════
        // КОНСТРУКТОР
        // ════════════════════════════════════════════════════════════

        public StaffViewModel()
        {
            StaffList = new ObservableCollection<Users>();

            // ✅ ИСПРАВЛЕНО: Оборачиваем в лямбды для совместимости с RelayCommand
            AddStaffCommand = new RelayCommand(_ => AddStaff());
            EditStaffCommand = new RelayCommand<Users>(EditStaff);
            DeleteStaffCommand = new RelayCommand<Users>(DeleteStaff);

            System.Diagnostics.Debug.WriteLine("[STAFF VM] Конструктор выполнен");
        }

        // ════════════════════════════════════════════════════════════
        // ЗАГРУЗКА ДАННЫХ
        // ════════════════════════════════════════════════════════════

        public void RefreshData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[STAFF VM] Начало загрузки данных...");

                using (var context = new AgroCultureEntities())
                {
                    var users = context.Users
                        .Where(u => u.IsActive && (u.Role == "admin" || u.Role == "manager"))
                        .OrderBy(u => u.Surname)
                        .ThenBy(u => u.FirstName)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"[STAFF VM] Загружено {users.Count} сотрудников");

                    StaffList.Clear();

                    int rowNumber = 1;
                    foreach (var user in users)
                    {
                        user.RowNumber = rowNumber++;
                        StaffList.Add(user);
                    }

                    // Статистика
                    TotalCount = users.Count;
                    AdminCount = users.Count(u => u.Role == "admin");
                    ManagerCount = users.Count(u => u.Role == "manager");

                    System.Diagnostics.Debug.WriteLine($"[STAFF VM] Админов: {AdminCount}, Менеджеров: {ManagerCount}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STAFF VM] ОШИБКА загрузки: {ex.Message}");
                ShowNotificationEvent($"Ошибка загрузки: {ex.Message}", false);
            }
        }

        // ════════════════════════════════════════════════════════════
        // ДОБАВЛЕНИЕ СОТРУДНИКА
        // ════════════════════════════════════════════════════════════

        private void AddStaff()
        {
            System.Diagnostics.Debug.WriteLine("[STAFF VM] Попытка добавить сотрудника...");

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedRole))
            {
                ShowNotificationEvent("Выберите роль", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewLogin))
            {
                ShowNotificationEvent("Введите логин", false);
                return;
            }

            // ✅ ВАЛИДАЦИЯ ОТДЕЛЬНЫХ ПОЛЕЙ ФИО
            if (string.IsNullOrWhiteSpace(NewSurname))
            {
                ShowNotificationEvent("Введите фамилию", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewFirstName))
            {
                ShowNotificationEvent("Введите имя", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 4)
            {
                ShowNotificationEvent("Пароль должен содержать минимум 4 символа", false);
                return;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // Проверка уникальности логина
                    if (context.Users.Any(u => u.Username == NewLogin.Trim()))
                    {
                        ShowNotificationEvent($"Логин '{NewLogin}' уже занят", false);
                        return;
                    }

                    // ✅ Создание нового пользователя с отдельными полями
                    var newUser = new Users
                    {
                        Username = NewLogin.Trim(),
                        PasswordHash = NewPassword,
                        Role = SelectedRole,
                        Surname = NewSurname.Trim(),
                        FirstName = NewFirstName.Trim(),
                        MiddleName = string.IsNullOrWhiteSpace(NewMiddleName) ? "" : NewMiddleName.Trim(),
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        Phone = "",
                        Email = ""
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"[STAFF VM] ✅ Сотрудник добавлен: {newUser.FullName}");

                    ShowNotificationEvent($"Сотрудник {newUser.FullName} успешно добавлен", true);

                    // Очистка формы
                    NewLogin = string.Empty;
                    NewSurname = string.Empty;
                    NewFirstName = string.Empty;
                    NewMiddleName = string.Empty;
                    NewPassword = string.Empty;
                    SelectedRole = "manager";

                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STAFF VM] ❌ Ошибка добавления: {ex.Message}");
                ShowNotificationEvent($"Ошибка: {ex.Message}", false);
            }
        }

        // ════════════════════════════════════════════════════════════
        // РЕДАКТИРОВАНИЕ СОТРУДНИКА
        // ════════════════════════════════════════════════════════════

        private void EditStaff(Users user)
        {
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("[STAFF VM] ❌ EditStaff: user == null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[STAFF VM] Запрос на редактирование: {user.Username} (ID: {user.UserId})");

            // Проверка: нельзя редактировать самого себя
            if (App.CurrentUser != null && user.UserId == App.CurrentUser.UserId)
            {
                ShowNotificationEvent("Невозможно редактировать свою учетную запись отсюда. Используйте профиль.", false);
                return;
            }

            // Вызываем событие для открытия окна редактирования
            RequestEdit?.Invoke(user);
        }

        // ════════════════════════════════════════════════════════════
        // УДАЛЕНИЕ СОТРУДНИКА
        // ════════════════════════════════════════════════════════════

        private void DeleteStaff(Users user)
        {
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("[STAFF VM] ❌ DeleteStaff: user == null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[STAFF VM] Попытка удалить: {user.Username} (ID: {user.UserId})");

            // Проверка: нельзя удалить самого себя
            if (App.CurrentUser != null && user.UserId == App.CurrentUser.UserId)
            {
                ShowNotificationEvent("Нельзя удалить свою учетную запись", false);
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить сотрудника:\n\n{user.FullName} ({user.Username})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                System.Diagnostics.Debug.WriteLine("[STAFF VM] Удаление отменено пользователем");
                return;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var userToDelete = context.Users.FirstOrDefault(u => u.UserId == user.UserId);

                    if (userToDelete != null)
                    {
                        // Мягкое удаление (деактивация)
                        userToDelete.IsActive = false;
                        context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"[STAFF VM] ✅ Сотрудник деактивирован: {user.FullName}");

                        ShowNotificationEvent($"Сотрудник {user.FullName} успешно удалён", true);

                        RefreshData();
                    }
                    else
                    {
                        ShowNotificationEvent("Сотрудник не найден", false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STAFF VM] ❌ Ошибка удаления: {ex.Message}");
                ShowNotificationEvent($"Ошибка удаления: {ex.Message}", false);
            }
        }

        // ════════════════════════════════════════════════════════════
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ════════════════════════════════════════════════════════════

        private void ShowNotificationEvent(string message, bool isSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"[STAFF VM] Уведомление: {message}");
            ShowNotification?.Invoke(message, isSuccess);
        }
    }
}