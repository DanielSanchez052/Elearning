using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;
using ELearning.Infrastructure.Seeding;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ELearning.Tests.Unit.Infrastructure.Seeding;

/// <summary>
/// S2: the development seeder must never run outside Development. A mistake
/// here would write shared demo accounts into a real database.
/// </summary>
public class DevDataSeederEnvironmentGuardTests
{
    private static Mock<DevDataSeeder> CreateSeederMock() =>
        new(
            Mock.Of<ICountryRepository>(),
            Mock.Of<IUserRepository>(),
            Mock.Of<IPasswordHasherService>(),
            NullLogger<DevDataSeeder>.Instance)
        {
            // The guard itself must run for real (that is what S2 is about),
            // while the actual database-writing seed is stubbed out.
            CallBase = true
        };

    [Fact]
    public async Task SeedForEnvironmentAsync_InDevelopment_Sows()
    {
        var seeder = CreateSeederMock();
        seeder.Setup(s => s.SeedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await seeder.Object.SeedForEnvironmentAsync(DevDataSeeder.DevelopmentEnvironmentName);

        seeder.Verify(s => s.SeedAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    [InlineData("QA")]
    [InlineData("")]
    public async Task SeedForEnvironmentAsync_OutsideDevelopment_NeverSows(string environment)
    {
        var seeder = CreateSeederMock();

        await seeder.Object.SeedForEnvironmentAsync(environment);

        seeder.Verify(s => s.SeedAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
