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
        private readonly IPasswordHashService _passwordHashService;
        private readonly AccessService _accessService;

        public UserService(
            IUserRepository userRepository,
            IBranchRepository branchRepository,
            IPasswordHashService passwordHashService,
            AccessService accessService)
        {
            _userRepository = userRepository;
            _branchRepository = branchRepository;
            _passwordHashService = passwordHashService;
            _accessService = accessService;
        }

        // Empleados de las sucursales del dueño
        public List<UserResponse> GetEmployees(string ownerCuit)
        {
            return _accessService.GetBranches(ownerCuit)
                .SelectMany(b => _userRepository.GetByBranch(b.Id))
                .Select(ResponseMapper.ToResponse)
                .ToList();
        }

        // Cada usuario ve su propio perfil; el dueño, además, el de sus empleados
        public UserResponse GetByCuit(string requesterCuit, string cuit)
        {
            User? user = _userRepository.GetByCuit(cuit);
            if (user is null || !user.IsActive)
            {
                throw new NotFoundException("El usuario no existe.");
            }

            if (cuit != requesterCuit)
            {
                if (_accessService.GetUser(requesterCuit).Role != UserRole.Owner || user.BranchId is null)
                {
                    throw new NotAllowedException("No tenés acceso a ese usuario.");
                }

                _accessService.EnsureBranchAccess(requesterCuit, user.BranchId.Value);
            }

            return ResponseMapper.ToResponse(user);
        }

        // Sin usuario logueado solo se puede registrar un dueño; un empleado lo crea el dueño de su sucursal
        public UserResponse Create(string? requesterCuit, UserCreateRequest request)
        {
            if (!CuitFormat.IsMatch(request.Cuit))
            {
                throw new AppValidationException("El CUIT debe tener el formato 20-12345678-9.");
            }

            if (_userRepository.GetByCuit(request.Cuit) is not null)
            {
                throw new ConflictException("Ya existe un usuario con ese CUIT.");
            }

            User newUser = new User
            {
                Cuit = request.Cuit,
                FirstName = request.FirstName,
                LastName = request.LastName,
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
            newUser.PasswordHash = _passwordHashService.Hash(newUser, request.Password);

            return ResponseMapper.ToResponse(_userRepository.Create(newUser));
        }

        // Edita un empleado de una sucursal del dueño; la contraseña solo se cambia si viene en el body
        public void Update(string ownerCuit, string cuit, UserUpdateRequest request)
        {
            User user = GetEmployeeOfOwner(ownerCuit, cuit);

            if (request.BranchId is null)
            {
                throw new AppValidationException("Un usuario Employee debe tener una sucursal asignada.");
            }

            _accessService.EnsureBranchAccess(ownerCuit, request.BranchId.Value);

            bool changesPassword = !string.IsNullOrWhiteSpace(request.Password);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.BranchId = request.BranchId;
            if (changesPassword)
            {
                user.PasswordHash = _passwordHashService.Hash(user, request.Password!);
            }

            _userRepository.Update(user);
        }

        // Baja lógica
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
