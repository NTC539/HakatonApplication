using HakatonApplication.Context;
using HakatonApplication.DTO;
using HakatonApplication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace HakatonApplication.Service
{
    public interface IAuthService
    {
        Task<int?> LoginAsync(string email, string password);
        Task<int?> RegisterAsync(RegisterDto registerDto);
        Task<UserInfoDto> GetUserInfoAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly HakatonDbContext _context;

        public AuthService(HakatonDbContext context)
        {
            _context = context;
        }

        public async Task<int?> LoginAsync(string email, string password)
        {
            // Ищем контакт по email
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Email == email);
            if (contact == null) return null;

            // Ищем пользователя по contact_id
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ContactId == contact.Id);
            if (user == null) return null;

            // Хэшируем введённый пароль с солью из БД
            string hashedInput = ComputeSha256Hex(user.Salt + password);
            if (hashedInput == user.Password)
                return user.Id;

            return null;
        }

        public async Task<int?> RegisterAsync(RegisterDto registerDto)
        {
            // Проверка, существует ли email
            var existingContact = await _context.Contacts.FirstOrDefaultAsync(c => c.Email == registerDto.Email);
            if (existingContact != null)
                return null; // email уже используется

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Добавляем контакт
                var contact = new Contact
                {
                    Email = registerDto.Email,
                    PhoneNumber = string.IsNullOrWhiteSpace(registerDto.Phone) ? null : registerDto.Phone
                };
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                // 2. Генерируем соль и хэш пароля
                var salt = GenerateSalt();
                var hashedPassword = ComputeSha256Hex(salt + registerDto.Password);

                // 3. Добавляем пользователя
                var user = new User
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Patronymic = registerDto.Patronymic,
                    Password = hashedPassword,
                    Salt = salt,
                    RegistrationDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddHours(3),
                    ContactId = contact.Id,
                    IsPublic = 0,
                    IsAdmin = 0
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();


                await transaction.CommitAsync();
                return user.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private string GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[8]; // 8 байт = 16 hex символов
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }

        private string ComputeSha256Hex(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
        public async Task<UserInfoDto> GetUserInfoAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Contact)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            return new UserInfoDto
            {
                Id = user.Id,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Patronymic = user.Patronymic ?? "",
                Email = user.Contact?.Email ?? "",
                Phone = user.Contact?.PhoneNumber ?? "",
                IsPublic = user.IsPublic == 1,
                IsAdmin = user.IsAdmin == 1
            };
        }

    }
}