using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infraestructure.Services
{
    public class AuthenticationService : ICustomAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly AuthenticationServiceOptions _options;

        public AuthenticationService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IOptions<AuthenticationServiceOptions> options)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _options = options.Value;
        }

        // Devuelve el usuario si existe y la contraseña le corresponde
        private User? ValidateUser(AuthenticationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cuit) || string.IsNullOrEmpty(request.Password))
                return null;

            User? user = _userRepository.GetByCuit(request.Cuit);

            // Usuarios creados antes de tener contraseña no pueden loguearse
            if (user is null || string.IsNullOrEmpty(user.PasswordHash))
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

        public string Authenticate(AuthenticationRequest request)
        {
            // Paso 1: validamos las credenciales
            User? user = ValidateUser(request);
            if (user is null)
            {
                throw new UnauthorizedException("CUIT o contraseña incorrectos.");
            }

            // Paso 2: creamos el token
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.SecretForKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // El payload solo va codificado en Base64 (cualquiera puede leerlo): nada sensible en los claims
            var claimsForToken = new List<Claim>
            {
                new Claim("sub", user.Cuit),               // identificador único del usuario
                new Claim("given_name", user.FirstName),
                new Claim("family_name", user.LastName),
                new Claim("role", user.Role.ToString())
            };

            var jwtSecurityToken = new JwtSecurityToken(
                _options.Issuer,
                _options.Audience,
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        public class AuthenticationServiceOptions
        {
            public const string AuthenticationService = "AuthenticationService";

            public string Issuer { get; set; } = string.Empty;
            public string Audience { get; set; } = string.Empty;
            public string SecretForKey { get; set; } = string.Empty;
        }
    }
}
