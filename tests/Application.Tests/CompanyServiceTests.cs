using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class CompanyServiceTests
    {
        [Fact]
        public void RemoveOwner_UnicoDuenioActivo_LanzaValidacionYNoLoQuita()
        {
            TestScenario scenario = new();

            Assert.Throws<AppValidationException>(() =>
                scenario.CompanyService.RemoveOwner(scenario.Company.Id, TestScenario.OwnerCuit));
            Assert.Single(scenario.Company.Owners);
        }

        [Fact]
        public void RemoveOwner_OtroDuenioInactivo_LanzaValidacion()
        {
            TestScenario scenario = new();
            scenario.Company.Owners.Add(new User { Cuit = "20222222223", Role = UserRole.Owner, IsActive = false });

            Assert.Throws<AppValidationException>(() =>
                scenario.CompanyService.RemoveOwner(scenario.Company.Id, TestScenario.OwnerCuit));
        }

        [Fact]
        public void RemoveOwner_ConOtroDuenioActivo_LoQuita()
        {
            TestScenario scenario = new();
            User partner = scenario.Users.Create(new User { Cuit = "20222222223", Role = UserRole.Owner });
            scenario.Company.Owners.Add(partner);

            scenario.CompanyService.RemoveOwner(scenario.Company.Id, TestScenario.OwnerCuit);

            Assert.Equal(new[] { partner.Cuit }, scenario.Company.Owners.Select(o => o.Cuit));
        }
    }
}
