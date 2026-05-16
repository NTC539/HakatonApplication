using HakatonApplication.Context;
using HakatonApplication.DTO;
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
            // Вызов хранимой функции create_user через raw SQL
            var sql = "SELECT create_user({0}, {1}, {2}, {3}, {4}, {5})";
            var parameters = new object[]
            {
                registerDto.Email,
                registerDto.Password,
                registerDto.FirstName,
                registerDto.LastName,
                registerDto.Patronymic,
                string.IsNullOrWhiteSpace(registerDto.Phone) ? DBNull.Value : (object)registerDto.Phone
            };
            var result = await _context.Database.SqlQueryRaw<int?>(sql, parameters).FirstOrDefaultAsync();
            return result;
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
                IsPublic = user.IsPublic == 1
            };
        }

        private string ComputeSha256Hex(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}