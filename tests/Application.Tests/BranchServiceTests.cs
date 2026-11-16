using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class BranchServiceTests
    {
        [Fact]
        public void Delete_DaDeBajaLaSucursalYSusEmpleados()
        {
            TestScenario scenario = new();
            scenario.Users.Create(new User { Cuit = "20-33333333-3", Role = UserRole.Employee, BranchId = scenario.Branch.Id });

            scenario.BranchService.Delete(TestScenario.OwnerCuit, scenario.Branch.Id);

            Assert.False(scenario.Branch.IsActive);
            Assert.False(scenario.Users.GetByCuit("20-33333333-3")!.IsActive);
        }

        [Fact]
        public void Delete_SucursalAjena_LanzaNotAllowedYNoBorra()
        {
            TestScenario scenario = new();

            Assert.Throws<NotAllowedException>(() => scenario.BranchService.Delete(TestScenario.OutsiderCuit, scenario.Branch.Id));
            Assert.True(scenario.Branch.IsActive);
        }

        [Fact]
        public void DeleteDeEmpresa_DaDeBajaSucursalesYEmpleados()
        {
            TestScenario scenario = new();
            scenario.Users.Create(new User { Cuit = "20-33333333-3", Role = UserRole.Employee, BranchId = scenario.Branch.Id });

            scenario.CompanyService.Delete(scenario.Company.Id);

            Assert.False(scenario.Company.IsActive);
            Assert.False(scenario.Branch.IsActive);
            Assert.False(scenario.Users.GetByCuit("20-33333333-3")!.IsActive);
        }
    }
}
