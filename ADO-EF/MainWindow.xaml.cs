﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;
using ADO_EF.Data;
using ADO_EF.Data.Entities;
using ADO_EF.Services;


namespace ADO_EF
{
    public partial class MainWindow : Window
    {
        private readonly DataContext _context;
        private readonly DatabaseService _dbService;
        private readonly EmailService _emailService;
        private string? _verificationCode;
        private string? _email;
        private User? _currentUser;
        private UserAccess? _currentUserAccess;

        public MainWindow(DatabaseService dbService, EmailService emailService, DataContext context)
        {
            InitializeComponent();
            _dbService = dbService;
            _emailService = emailService;
            _context = context;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LoginBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Введите логин и пароль!");
                    return;
                }

                var user = await _dbService.AuthenticateAsync(LoginBox.Text, PasswordBox.Password);
                if (user != null)
                {
                    _currentUser = user;
                    _currentUserAccess = await _context.UserAccesses
                        .FirstOrDefaultAsync(ua => ua.UserId == user.Id);
                    WelcomeMessage.Text = $"Добро пожаловать, {user.Name}!";
                    LoginPanel.Visibility = Visibility.Collapsed;
                    WelcomePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}");
            }
        }

        private void ToRegister_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            EmailPanel.Visibility = Visibility.Visible;
        }

        private async void SendCode_Click(object sender, RoutedEventArgs e)
        {
            _email = EmailBox.Text;
            if (string.IsNullOrWhiteSpace(_email))
            {
                MessageBox.Show("Введите email!");
                return;
            }

            if (!IsValidEmail(_email))
            {
                MessageBox.Show("Введите корректный email, например, example@domain.com");
                return;
            }

            if (await _dbService.IsEmailRegisteredAsync(_email))
            {
                MessageBox.Show("Этот email уже зарегистрирован!");
                return;
            }

            try
            {
                _verificationCode = _emailService.SendVerificationCode(_email);
                CodeBox1.Text = CodeBox2.Text = CodeBox3.Text = CodeBox4.Text = CodeBox5.Text = CodeBox6.Text = "";
                EmailPanel.Visibility = Visibility.Collapsed;
                CodePanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке кода: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private void CodeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void CodeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox currentBox = sender as TextBox;
            if (currentBox == null || currentBox.Text.Length != 1) return;

            if (currentBox == CodeBox1) CodeBox2.Focus();
            else if (currentBox == CodeBox2) CodeBox3.Focus();
            else if (currentBox == CodeBox3) CodeBox4.Focus();
            else if (currentBox == CodeBox4) CodeBox5.Focus();
            else if (currentBox == CodeBox5) CodeBox6.Focus();
            else if (currentBox == CodeBox6)
            {
                VerifyCode_Click(this, new RoutedEventArgs());
            }
        }

        private void VerifyCode_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = $"{CodeBox1.Text}{CodeBox2.Text}{CodeBox3.Text}{CodeBox4.Text}{CodeBox5.Text}{CodeBox6.Text}";

            if (string.IsNullOrWhiteSpace(enteredCode) || enteredCode.Length != 6)
            {
                MessageBox.Show("Введите 6-значный код!");
                return;
            }

            if (enteredCode == _verificationCode)
            {
                ConfirmedEmailBox.Text = _email ?? string.Empty;
                CodePanel.Visibility = Visibility.Collapsed;
                RegisterPanel.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Неверный код!");
            }
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text) ||
                string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text) ||
                string.IsNullOrWhiteSpace(RegPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Заполните обязательные поля (логин, имя, телефон, пароль, подтверждение пароля)!");
                return;
            }

            if (RegPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            if (await _dbService.IsUsernameRegisteredAsync(UsernameBox.Text))
            {
                MessageBox.Show("Этот логин уже занят!");
                return;
            }

            Guid userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Name = NameBox.Text,
                Email = _email ?? string.Empty,
                Phonenumber = PhoneBox.Text,
                RegisteredAt = DateTime.Now
            };

            string salt = Random.Shared.Next().ToString();
            var userAccess = new UserAccess
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Login = UsernameBox.Text,
                Salt = salt,
                Dk = Kdf(RegPasswordBox.Password, salt),
                RoleId = "SelfRegistered"
            };

            try
            {
                await _dbService.RegisterUserAsync(user, userAccess);
                MessageBox.Show("Регистрация успешна! Войдите в систему.");
                RegisterPanel.Visibility = Visibility.Collapsed;
                LoginPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}");
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUserAccess == null) return;

            _email = _currentUser.Email;
            ConfirmedEmailBox.Text = _email;
            UsernameBox.Text = _currentUserAccess.Login;
            NameBox.Text = _currentUser.Name;
            PhoneBox.Text = _currentUser.Phonenumber;
            RegPasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";

            WelcomePanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            if (MessageBox.Show("Точно удалить пользователя?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _currentUser.Name = "";
                _currentUser.Email = "";
                _currentUser.Phonenumber = "";
                _currentUser.Birthdate = null;
                _currentUser.DeletedAt = DateTime.Now;

                try
                {
                    await _context.SaveChangesAsync();
                    MessageBox.Show("Данные удалены!");
                    _currentUser = null;
                    _currentUserAccess = null;
                    WelcomePanel.Visibility = Visibility.Collapsed;
                    LoginPanel.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            EmailPanel.Visibility = Visibility.Collapsed;
            CodePanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }

        private void BackToEmailFromCodeButton_Click(object sender, RoutedEventArgs e)
        {
            CodePanel.Visibility = Visibility.Collapsed;
            EmailPanel.Visibility = Visibility.Visible;
            CodeBox1.Text = CodeBox2.Text = CodeBox3.Text = CodeBox4.Text = CodeBox5.Text = CodeBox6.Text = "";
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _currentUser = null;
            _currentUserAccess = null;
            WelcomePanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            var paletteHelper = new PaletteHelper();
            var currentTheme = paletteHelper.GetTheme();
            var newTheme = currentTheme.GetBaseTheme() == BaseTheme.Dark ? "Light" : "Dark";
            app.ChangeTheme(newTheme);
        }

        private string Kdf(string password, string salt)
        {
            int c = 3;
            int dklen = 20;
            string t = password + salt;

            for (int i = 0; i < c; i++)
            {
                t = Hash(t);
            }
            return t.Substring(0, dklen);
        }

        private string Hash(string input)
        {
            return Convert.ToHexString(
                System.Security.Cryptography.SHA1.HashData(
                    System.Text.Encoding.UTF8.GetBytes(input)));
        }
    }
}