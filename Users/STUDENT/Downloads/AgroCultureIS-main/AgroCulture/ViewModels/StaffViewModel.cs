using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AgroCulture.Services;
using AgroCulture.Commands;

namespace AgroCulture.ViewModels
{
    public class StaffViewModel : BaseViewModel
    {
        // ═══════════════════════════════════════════════════════════
        // ✅ SINGLETON PATTERN (THREAD-SAFE)
        // ═══════════════════════════════════════════════════════════
        private static StaffViewModel _instance;
        private static readonly object _lock = new object();

        public static StaffViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new StaffViewModel();
                        }
                    }
                }
                return _instance;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // WRAPPER-КЛАСС для Users с порядковым номером
        // ═══════════════════════════════════════════════════════════
        public class UserWithRowNumber
        {
            public int RowNumber { get; set; }
            public Users User { get; set; }

            // Прокси-свойства для биндинга
            public int UserId => User.UserId;
            public string Username => User.Username;
            public string FullName => User.FullName;
            public string Role => User.Role;
            public DateTime CreatedAt => User.CreatedAt;
            public bool IsActive => User.IsActive;
        }

        // ═══════════════════════════════════════════════════════════
        // СВОЙСТВА
        // ═══════════════════════════════════════════════════════════

        private string _selectedRole = "manager";
        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        private string _newLogin;
        public string NewLogin
        {
            get => _newLogin;
            set => SetProperty(ref _newLogin, value);
        }

        private string _newFullName;
        public string NewFullName
        {
            get => _newFullName;
            set => SetProperty(ref _newFullName, value);
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _newPhone;
        public string NewPhone
        {
            get => _newPhone;
            set => SetProperty(ref _newPhone, value);
        }

        private string _newEmail;
        public string NewEmail
        {
            get => _newEmail;
            set => SetProperty(ref _newEmail, value);
        }

        private ObservableCollection<UserWithRowNumber> _staffList;
        public ObservableCollection<UserWithRowNumber> StaffList
        {
            get => _staffList;
            set => SetProperty(ref _staffList, value);
        }

        private int _adminCount;
        public int AdminCount
        {
            get => _adminCount;
            set => SetProperty(ref _adminCount, value);
        }

        private int _managerCount;
        public int ManagerCount
        {
            get => _managerCount;
            set => SetProperty(ref _managerCount, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        // ═══════════════════════════════════════════════════════════
        // КОМАНДЫ
        // ═══════════════════════════════════════════════════════════

        public RelayCommand AddStaffCommand { get; }
        public RelayCommand<UserWithRowNumber> EditStaffCommand { get; }
        public RelayCommand<UserWithRowNumber> DeleteStaffCommand { get; }

        public event Action<string, bool> ShowNotification;

        // ═══════════════════════════════════════════════════════════
        // ✅ ПРИВАТНЫЙ КОНСТРУКТОР (для Singleton)
        // ═══════════════════════════════════════════════════════════

        private StaffViewModel()
        {
            StaffList = new ObservableCollection<UserWithRowNumber>();

            AddStaffCommand = new RelayCommand(_ => AddStaff());
            EditStaffCommand = new RelayCommand<UserWithRowNumber>(EditStaff);
            DeleteStaffCommand = new RelayCommand<UserWithRowNumber>(DeleteStaff);

            // ✅ ЗАГРУЖАЕМ ДАННЫЕ ОДИН РАЗ ПРИ СОЗДАНИИ
            LoadStaff();
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ ПУБЛИЧНЫЙ МЕТОД ДЛЯ ОБНОВЛЕНИЯ ДАННЫХ
        // ═══════════════════════════════════════════════════════════

        public void RefreshData()
        {
            LoadStaff();
        }

        // ═══════════════════════════════════════════════════════════
        // ЗАГРУЗКА ДАННЫХ
        // ═══════════════════════════════════════════════════════════

        public void LoadStaff()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var staff = context.Users
                        .Where(u => u.IsActive && (u.Role == "admin" || u.Role == "manager"))
                        .OrderBy(u => u.UserId)
                        .ToList();

                    StaffList.Clear();

                    int rowNumber = 1;
                    foreach (var user in staff)
                    {
                        StaffList.Add(new UserWithRowNumber
                        {
                            RowNumber = rowNumber++,
                            User = user
                        });
                    }

                    UpdateStatistics();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ДОБАВЛЕНИЕ СОТРУДНИКА (БЕЗ ХЕШИРОВАНИЯ)
        // ═══════════════════════════════════════════════════════════

        private void AddStaff()
        {
            if (string.IsNullOrWhiteSpace(NewLogin) ||
                string.IsNullOrWhiteSpace(NewFullName) ||
                string.IsNullOrWhiteSpace(NewPassword))
            {
                ShowNotification?.Invoke("Заполните все обязательные поля", false);
                return;
            }

            if (NewPassword.Length < 4)
            {
                ShowNotification?.Invoke("Пароль должен содержать минимум 4 символа", false);
                return;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    if (context.Users.Any(u => u.Username.ToLower() == NewLogin.ToLower().Trim()))
                    {
                        ShowNotification?.Invoke("Пользователь с таким логином уже существует", false);
                        return;
                    }

                    // ✅ ИСПРАВЛЕНО: Используем свойства из ViewModel
                    var newUser = new Users
                    {
                        Username = NewLogin.Trim(),
                        FullName = NewFullName.Trim(),
                        Role = SelectedRole,
                        PasswordHash = NewPassword.Trim(),
                        Phone = string.IsNullOrWhiteSpace(NewPhone) ? null : NewPhone.Trim(),  // ✅ ИСПРАВЛЕНО
                        Email = string.IsNullOrWhiteSpace(NewEmail) ? null : NewEmail.Trim(),  // ✅ ИСПРАВЛЕНО
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    StaffList.Add(new UserWithRowNumber
                    {
                        RowNumber = StaffList.Count + 1,
                        User = newUser
                    });

                    UpdateStatistics();
                    ClearForm();

                    string roleText = SelectedRole == "admin" ? "Администратор" : "Менеджер";
                    ShowNotification?.Invoke($"{roleText} {newUser.FullName} успешно добавлен!", true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка добавления сотрудника:\n{ex.Message}\n\nВнутренняя ошибка:\n{ex.InnerException?.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // РЕДАКТИРОВАНИЕ СОТРУДНИКА
        // ═══════════════════════════════════════════════════════════

        private void EditStaff(UserWithRowNumber userWrapper)
        {
            if (userWrapper?.User == null) return;

            try
            {
                var window = new Views.StaffEditWindow(userWrapper.User.UserId);
                window.Owner = Application.Current.MainWindow;

                window.ShowDialog();

                if (window.DialogResultSuccess)
                {
                    LoadStaff();
                    ShowNotification?.Invoke("✅ Данные сотрудника обновлены", true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // УДАЛЕНИЕ СОТРУДНИКА
        // ═══════════════════════════════════════════════════════════

        private void DeleteStaff(UserWithRowNumber userWrapper)
        {
            if (userWrapper?.User == null) return;

            var user = userWrapper.User;

            if (App.CurrentUser != null && user.UserId == App.CurrentUser.UserId)
            {
                ShowNotification?.Invoke("Нельзя удалить самого себя", false);
                return;
            }

            if (user.Role == "admin" && StaffList.Count(w => w.User.Role == "admin") <= 1)
            {
                ShowNotification?.Invoke("Нельзя удалить последнего администратора", false);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить сотрудника {user.FullName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var existingUser = context.Users.Find(user.UserId);
                    if (existingUser != null)
                    {
                        existingUser.IsActive = false;
                        context.SaveChanges();

                        StaffList.Remove(userWrapper);
                        RenumberRows();
                        UpdateStatistics();

                        string roleText = user.Role == "admin" ? "Администратор" : "Менеджер";
                        ShowNotification?.Invoke($"{roleText} {user.FullName} удален из системы", true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ═══════════════════════════════════════════════════════════

        private void RenumberRows()
        {
            for (int i = 0; i < StaffList.Count; i++)
            {
                StaffList[i].RowNumber = i + 1;
            }
        }

        private void UpdateStatistics()
        {
            AdminCount = StaffList.Count(w => w.User.Role == "admin");
            ManagerCount = StaffList.Count(w => w.User.Role == "manager");
            TotalCount = StaffList.Count;
        }

        private void ClearForm()
        {
            NewLogin = string.Empty;
            NewFullName = string.Empty;
            NewPassword = string.Empty;
            NewPhone = string.Empty;   
            NewEmail = string.Empty;
            SelectedRole = "manager";
        }
    }
}