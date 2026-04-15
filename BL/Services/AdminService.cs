using DatabaseModel;
using DatabaseModel.Models;
using System;
using System.Linq;

namespace BL.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void CreateAdmin(string name, string email, string password)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var admin = new Admin
            {
                Name = name,
                Email = email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.Now
            };

            _context.Admins.Add(admin);
            _context.SaveChanges();
        }

        public bool ValidateLogin(string email, string password)
        {
            var admin = _context.Admins
                .FirstOrDefault(a => a.Email == email);

            if (admin == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
        }
    }
}