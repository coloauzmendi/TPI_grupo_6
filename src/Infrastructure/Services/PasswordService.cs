using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infraestructure.Services
{
    // Implementa el hasheo con el PasswordHasher de ASP.NET Identity (PBKDF2 con salt)
    public class PasswordService : IPasswordService
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public PasswordService(IPasswordHasher<User> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public string Hash(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }
    }
}
