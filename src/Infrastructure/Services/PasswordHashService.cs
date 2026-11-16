using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    // Hash de contraseñas con el algoritmo de ASP.NET Core Identity
    public class PasswordHashService : IPasswordHashService
    {
        private readonly PasswordHasher<User> _passwordHasher = new();

        public string Hash(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public bool Verify(User user, string password)
        {
            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
        }
    }
}
