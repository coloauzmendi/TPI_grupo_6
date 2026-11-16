using System.Text.RegularExpressions;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class UserService
    {
        private static readonly Regex CuitFormat = new(@"^\d{2}-\d{8}-\d$");

        private readonly IUserRepository _userRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IPasswordService _passwordService;
        private readonly AccessService _accessService;

        public UserService(
            IUserRepository userRepository,
            IBranchRepository branchRepository,
            IPasswordService passwordService,
            AccessService accessService)
        {
            _userRepository = userRepository;
            _branchRepository = branchRepository;
            _passwordService = passwordService;
            _accessService = accessService;
        }

        // Empleados de las sucursales del dueño
        public List<User> GetEmployees(string ownerCuit)
        {
            return _accessService.GetBranches(ownerCuit)
                .SelectMany(b => _userRepository.GetByBranch(b.Id))
                .ToList();
        }

        // Cada usuario ve su propio perfil; el dueño, además, el de sus empleados
        public User GetByCuit(string requesterCuit, string cuit)
        {
            User? user = _userRepository.GetByCuit(cuit);
            if (user is null || !user.IsActive)
            {
                throw new NotFoundException("El usuario no existe.");
            }

            if (cuit != requesterCuit)
            {
                User requester = _accessService.GetUser(requesterCuit);
                if (requester.Role != UserRole.Owner || user.BranchId is null)
                {
                    throw new NotAllowedException("No tenés acceso a ese usuario.");
                }

                _accessService.EnsureBranchAccess(requesterCuit, user.BranchId.Value);
            }

            return user;
        }

        // Registrar un dueño es público (sin eso nadie podría crear el primer usuario).
        // Crear un empleado exige estar logueado como dueño de su sucursal: requesterCuit es null si no hay sesión.
        public User Create(UserCreateRequest request, string? requesterCuit)
        {
            if (!CuitFormat.IsMatch(request.Cuit))
            {
                throw new AppValidationException("El CUIT debe tener el formato 20-12345678-9.");
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                throw new AppValidationException("El nombre y el apellido son obligatorios.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new AppValidationException("La contraseña es obligatoria.");
            }

            if (_userRepository.GetByCuit(request.Cuit) is not null)
            {
                throw new ConflictException("Ya existe un usuario con ese CUIT.");
            }

            User newUser = new User
            {
                Cuit = request.Cuit,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Role = request.Role,
                BranchId = request.BranchId
            };

            ValidateRoleAndBranch(newUser);

            if (newUser.Role == UserRole.Employee)
            {
                if (requesterCuit is null)
                {
                    throw new UnauthorizedException("Iniciá sesión como dueño para crear empleados.");
                }

                if (_accessService.GetUser(requesterCuit).Role != UserRole.Owner)
                {
                    throw new NotAllowedException("Solo un dueño puede crear empleados.");
                }

                _accessService.EnsureBranchAccess(requesterCuit, newUser.BranchId!.Value);
            }

            // Se guarda solo el hash, nunca la contraseña en texto plano
            newUser.PasswordHash = _passwordService.Hash(newUser, request.Password);

            return _userRepository.Create(newUser);
        }

        // Edita un empleado de una sucursal del dueño; la contraseña solo se cambia si viene informada
        public void Update(string ownerCuit, string cuit, UserUpdateRequest request)
        {
            User user = GetEmployeeOfOwner(ownerCuit, cuit);

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                throw new AppValidationException("El nombre y el apellido son obligatorios.");
            }

            if (request.BranchId is null)
            {
                throw new AppValidationException("Un usuario Employee debe tener una sucursal asignada.");
            }

            _accessService.EnsureBranchAccess(ownerCuit, request.BranchId.Value);

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.BranchId = request.BranchId;
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = _passwordService.Hash(user, request.Password);
            }

            _userRepository.Update(user);
        }

        // Baja lógica: el empleado no puede volver a iniciar sesión
        public void Delete(string ownerCuit, string cuit)
        {
            _userRepository.Delete(GetEmployeeOfOwner(ownerCuit, cuit));
        }

        // Empleado activo asignado a una sucursal a la que el dueño tiene acceso
        private User GetEmployeeOfOwner(string ownerCuit, string cuit)
        {
            User? user = _userRepository.GetByCuit(cuit);
            if (user is null || !user.IsActive || user.Role != UserRole.Employee)
            {
                throw new NotFoundException("El empleado no existe.");
            }

            _accessService.EnsureBranchAccess(ownerCuit, user.BranchId!.Value);

            return user;
        }

        // Employee debe tener una sucursal existente; Owner no tiene sucursal
        private void ValidateRoleAndBranch(User user)
        {
            if (user.Role == UserRole.Owner && user.BranchId is not null)
            {
                throw new AppValidationException("Un usuario Owner no puede tener sucursal.");
            }

            if (user.Role == UserRole.Employee)
            {
                if (user.BranchId is null)
                {
                    throw new AppValidationException("Un usuario Employee debe tener una sucursal asignada.");
                }

                if (_branchRepository.GetById(user.BranchId.Value) is null)
                {
                    throw new AppValidationException("La sucursal indicada no existe.");
                }
            }
        }
    }
}
