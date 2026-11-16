using Application.Models;
using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class UserServiceTests
    {
        private static UserCreateRequest Employee(Guid branchId) => new()
        {
            Cuit = "20-33333333-3",
            FirstName = "Lucia",
            LastName = "Diaz",
            Role = UserRole.Employee,
            BranchId = branchId,
            Password = "1234"
        };

        [Fact]
        public void Create_DuenoSinSesion_LoRegistraConLaContrasenaHasheada()
        {
            TestScenario scenario = new();
            UserCreateRequest request = new() { Cuit = "20-44444444-4", FirstName = "Pablo", LastName = "Ruiz", Role = UserRole.Owner, Password = "secreta" };

            User created = scenario.UserService.Create(request, requesterCuit: null);

            Assert.Equal("hash:secreta", created.PasswordHash);
            Assert.Contains(created, scenario.Users.Users);
        }

        [Fact]
        public void Create_EmpleadoPorElDuenoDeLaSucursal_LoCrea()
        {
            TestScenario scenario = new();

            User created = scenario.UserService.Create(Employee(scenario.Branch.Id), TestScenario.OwnerCuit);

            Assert.Equal(scenario.Branch.Id, created.BranchId);
        }

        [Fact]
        public void Create_EmpleadoSinSesion_LanzaUnauthorized()
        {
            TestScenario scenario = new();

            Assert.Throws<UnauthorizedException>(() => scenario.UserService.Create(Employee(scenario.Branch.Id), requesterCuit: null));
        }

        [Fact]
        public void Create_EmpleadoEnSucursalAjena_LanzaNotAllowed()
        {
            TestScenario scenario = new();

            Assert.Throws<NotAllowedException>(() => scenario.UserService.Create(Employee(scenario.Branch.Id), TestScenario.OutsiderCuit));
        }

        [Fact]
        public void Create_CuitRepetido_LanzaConflict()
        {
            TestScenario scenario = new();
            scenario.UserService.Create(Employee(scenario.Branch.Id), TestScenario.OwnerCuit);

            Assert.Throws<ConflictException>(() => scenario.UserService.Create(Employee(scenario.Branch.Id), TestScenario.OwnerCuit));
        }

        [Fact]
        public void Create_CuitConFormatoInvalido_LanzaValidacion()
        {
            TestScenario scenario = new();
            UserCreateRequest request = Employee(scenario.Branch.Id);
            request.Cuit = "20333333333";

            Assert.Throws<AppValidationException>(() => scenario.UserService.Create(request, TestScenario.OwnerCuit));
        }
    }
}
