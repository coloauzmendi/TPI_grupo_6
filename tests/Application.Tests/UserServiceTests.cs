using Application.Models;
using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class UserServiceTests
    {
        private const string EmployeeCuit = "20-33333333-3";

        private static UserCreateRequest EmployeeRequest(TestScenario scenario) => new()
        {
            Cuit = EmployeeCuit,
            FirstName = "Luis",
            LastName = "Diaz",
            Role = UserRole.Employee,
            BranchId = scenario.Branch.Id,
            Password = "secreta"
        };

        [Fact]
        public void Create_EmpleadoPorSuDuenio_GuardaSoloElHash()
        {
            TestScenario scenario = new();

            UserResponse created = scenario.UserService.Create(TestScenario.OwnerCuit, EmployeeRequest(scenario));

            Assert.Equal(EmployeeCuit, created.Cuit);
            Assert.Equal("hash:secreta", scenario.Users.GetByCuit(EmployeeCuit)!.PasswordHash);
        }

        [Fact]
        public void Create_CuitExistente_LanzaConflicto()
        {
            TestScenario scenario = new();
            scenario.UserService.Create(TestScenario.OwnerCuit, EmployeeRequest(scenario));

            Assert.Throws<ConflictException>(() => scenario.UserService.Create(TestScenario.OwnerCuit, EmployeeRequest(scenario)));
        }

        [Fact]
        public void Create_EmpleadoSinSesion_LanzaUnauthorized()
        {
            TestScenario scenario = new();

            Assert.Throws<UnauthorizedException>(() => scenario.UserService.Create(null, EmployeeRequest(scenario)));
        }

        [Fact]
        public void Create_EmpleadoEnSucursalAjena_LanzaNotAllowed()
        {
            TestScenario scenario = new();

            Assert.Throws<NotAllowedException>(() => scenario.UserService.Create(TestScenario.OutsiderCuit, EmployeeRequest(scenario)));
        }

        [Fact]
        public void Update_SinContrasenia_MantieneLaAnterior()
        {
            TestScenario scenario = new();
            scenario.UserService.Create(TestScenario.OwnerCuit, EmployeeRequest(scenario));
            User employee = scenario.Users.GetByCuit(EmployeeCuit)!;
            UserUpdateRequest request = new() { FirstName = "Luisa", LastName = "Diaz", BranchId = scenario.Branch.Id };

            scenario.UserService.Update(TestScenario.OwnerCuit, EmployeeCuit, request);

            Assert.Equal("Luisa", employee.FirstName);
            Assert.Equal("hash:secreta", employee.PasswordHash);
        }

        [Fact]
        public void GetByCuit_EmpleadoConsultaOtroUsuario_LanzaNotAllowed()
        {
            TestScenario scenario = new();
            scenario.UserService.Create(TestScenario.OwnerCuit, EmployeeRequest(scenario));

            Assert.Throws<NotAllowedException>(() => scenario.UserService.GetByCuit(EmployeeCuit, TestScenario.OwnerCuit));
        }
    }
}
