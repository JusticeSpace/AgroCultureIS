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

        // ✅ Отдельные поля для ФИО
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

        // ✅ НОВОЕ: Поля для контактной информации
        private string _newPhone;
        public string NewPhone
        {
            get => _newPhone;
            set { _newPhone = value; OnPropertyChanged(); }
        }

        private string _newEmail;
        public string NewEmail
        {
            get => _newEmail;
            set { _newEmail = value; OnPropertyChanged(); }
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

            // Валидация роли
            if (string.IsNullOrWhiteSpace(SelectedRole))
            {
                ShowNotificationEvent("❌ Выберите роль", false);
                return;
            }

            // ✅ Валидация логина через ValidationService
            var usernameValidation = Services.ValidationService.ValidateUsername(NewLogin);
            if (!usernameValidation.isValid)
            {
                ShowNotificationEvent(usernameValidation.errorMessage, false);
                return;
            }

            // ✅ Валидация ФИО
            var surnameValidation = Services.ValidationService.ValidateName(NewSurname, "Фамилия");
            if (!surnameValidation.isValid)
            {
                ShowNotificationEvent(surnameValidation.errorMessage, false);
                return;
            }

            var firstNameValidation = Services.ValidationService.ValidateName(NewFirstName, "Имя");
            if (!firstNameValidation.isValid)
            {
                ShowNotificationEvent(firstNameValidation.errorMessage, false);
                return;
            }

            // Отчество опционально, но если заполнено - валидируем
            if (!string.IsNullOrWhiteSpace(NewMiddleName))
            {
                var middleNameValidation = Services.ValidationService.ValidateName(NewMiddleName, "Отчество");
                if (!middleNameValidation.isValid)
                {
                    ShowNotificationEvent(middleNameValidation.errorMessage, false);
                    return;
                }
            }

            // ✅ Валидация пароля через ValidationService
            var passwordValidation = Services.ValidationService.ValidatePassword(NewPassword);
            if (!passwordValidation.isValid)
            {
                ShowNotificationEvent(passwordValidation.errorMessage, false);
                return;
            }

            // ✅ Валидация Email (опционально)
            if (!string.IsNullOrWhiteSpace(NewEmail))
            {
                var emailValidation = Services.ValidationService.ValidateEmail(NewEmail);
                if (!emailValidation.isValid)
                {
                    ShowNotificationEvent(emailValidation.errorMessage, false);
                    return;
                }
            }

            // ✅ Валидация телефона (опционально)
            if (!string.IsNullOrWhiteSpace(NewPhone))
            {
                var phoneValidation = Services.ValidationService.ValidatePhone(NewPhone);
                if (!phoneValidation.isValid)
                {
                    ShowNotificationEvent(phoneValidation.errorMessage, false);
                    return;
                }
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

                    var newUser = new Users
                    {
                        Username = NewLogin.Trim(),
                        PasswordHash = Services.PasswordHasher.HashPassword(NewPassword),
                        Role = SelectedRole,
                        Surname = NewSurname.Trim(),
                        FirstName = NewFirstName.Trim(),
                        MiddleName = string.IsNullOrWhiteSpace(NewMiddleName) ? "" : NewMiddleName.Trim(),
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        Phone = NewPhone?.Trim() ?? "",      // ✅ НОВОЕ
                        Email = NewEmail?.Trim() ?? ""       // ✅ НОВОЕ
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
                    NewPhone = string.Empty;      // ✅ НОВОЕ
                    NewEmail = string.Empty;      // ✅ НОВОЕ
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
        // ЗАМЕНИ этот метод в StaffViewModel.cs
        // ════════════════════════════════════════════════════════════

        private void DeleteStaff(Users user)
        {
            // ✅ НОВОЕ: Проверка на null
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("[STAFF VM] ❌ DeleteStaff: user == null");
                ShowNotificationEvent("Ошибка: пользователь не выбран", false);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[STAFF VM] Попытка удалить: {user.Username} (ID: {user.UserId})");

            // Проверка: нельзя удалить самого себя
            if (App.CurrentUser != null && user.UserId == App.CurrentUser.UserId)
            {
                ShowNotificationEvent("Нельзя удалить свою учетную запись", false);
                System.Diagnostics.Debug.WriteLine("[STAFF VM] ❌ Попытка удалить свой аккаунт!");
                return;
            }

            // Подтверждение удаления
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
                    // ✅ Ищем сотрудника в БД
                    var userToDelete = context.Users.FirstOrDefault(u => u.UserId == user.UserId);

                    if (userToDelete != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[STAFF VM] Найден пользователь в БД: {userToDelete.Username}");

                        // ✅ Мягкое удаление (деактивация)
                        userToDelete.IsActive = false;

                        System.Diagnostics.Debug.WriteLine("[STAFF VM] Устанавливаем IsActive = false");

                        // ✅ Сохраняем в БД
                        context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"[STAFF VM] ✅ Изменения сохранены в БД");

                        // ✅ КРИТИЧНО: Обновляем UI немедленно!
                        System.Diagnostics.Debug.WriteLine("[STAFF VM] Начинаем перезагрузку данных...");
                        RefreshData();  // ← ЭТО САМОЕ ВАЖНОЕ!
                        System.Diagnostics.Debug.WriteLine("[STAFF VM] ✅ Данные перезагружены");

                        // Показываем уведомление
                        ShowNotificationEvent($"Сотрудник {user.FullName} успешно удалён", true);

                        System.Diagnostics.Debug.WriteLine($"[STAFF VM] ✅✅✅ УСПЕШНО УДАЛЁН: {user.FullName}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[STAFF VM] ❌ Сотрудник НЕ НАЙДЕН в БД (ID: {user.UserId})");
                        ShowNotificationEvent("Сотрудник не найден в базе данных", false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STAFF VM] ❌ ОШИБКА при удалении: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[STAFF VM] StackTrace: {ex.StackTrace}");
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

        // ✅ НОВОЕ: Валидация Email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}