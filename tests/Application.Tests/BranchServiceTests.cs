using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class BranchServiceTests
    {
        [Fact]
        public void Create_EnEmpresaPropia_LaCrea()
        {
            TestScenario scenario = new();
            Branch branch = new() { CompanyId = scenario.Branch.CompanyId, Locality = "Funes", Address = "San Martin 100", Industry = Industry.Grocery };

            Branch created = scenario.BranchService.Create(TestScenario.OwnerCuit, branch);

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal(2, scenario.BranchService.GetAll(TestScenario.OwnerCuit).Count);
        }

        [Fact]
        public void Create_EnEmpresaAjena_LanzaNotAllowed()
        {
            TestScenario scenario = new();
            Branch branch = new() { CompanyId = scenario.Branch.CompanyId, Locality = "Funes", Address = "San Martin 100" };

            Assert.Throws<NotAllowedException>(() => scenario.BranchService.Create(TestScenario.OutsiderCuit, branch));
        }

        [Fact]
        public void Create_SinDireccion_LanzaValidacion()
        {
            TestScenario scenario = new();
            Branch branch = new() { CompanyId = scenario.Branch.CompanyId, Locality = "Funes", Address = " " };

            Assert.Throws<AppValidationException>(() => scenario.BranchService.Create(TestScenario.OwnerCuit, branch));
        }

        [Fact]
        public void Delete_DaDeBajaTambienASusEmpleados()
        {
            TestScenario scenario = new();
            scenario.Users.Create(new User { Cuit = "20-55555555-5", Role = UserRole.Employee, BranchId = scenario.Branch.Id });

            scenario.BranchService.Delete(TestScenario.OwnerCuit, scenario.Branch.Id);

            Assert.Empty(scenario.Users.GetByBranch(scenario.Branch.Id));
        }
    }
}
