using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ADO_EF.Data;
using ADO_EF.Data.Entities;

namespace ADO_EF
{
    public class DatabaseService
    {
        private readonly DataContext _context;

        public DatabaseService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при проверке email: {ex.Message}\nInner Exception: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<bool> IsUsernameRegisteredAsync(string login)
        {
            try
            {
                return await _context.UserAccesses.AnyAsync(ua => ua.Login == login);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при проверке логина: {ex.Message}\nInner Exception: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<User?> AuthenticateAsync(string login, string password)
        {
            try
            {
                var userAccess = await _context.UserAccesses.FirstOrDefaultAsync(ua => ua.Login == login);
                if (userAccess == null)
                {
                    return null;
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userAccess.UserId);
                if (user == null || user.DeletedAt != null)
                {
                    return null;
                }

                string salt = userAccess.Salt;
                string dk = Kdf(password, salt);
                if (dk != userAccess.Dk)
                {
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при аутентификации: {ex.Message}\nInner Exception: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task RegisterUserAsync(User user, UserAccess userAccess)
        {
            try
            {
                _context.Users.Add(user);
                _context.UserAccesses.Add(userAccess);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при регистрации пользователя: {ex.Message}\nInner Exception: {ex.InnerException?.Message}", ex);
            }
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