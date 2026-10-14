using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;
using ELearning.Infrastructure.Seeding;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ELearning.Tests.Unit.Infrastructure.Seeding;

public class DevDataSeederTests
{
    private readonly Mock<ICountryRepository> _countries = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasherService> _hasher = new();
    private readonly List<Country> _createdCountries = new();
    private readonly List<User> _createdUsers = new();

    /// <summary>
    /// Value the tests configure as the dev seed password. It is a marker, not
    /// a credential: the real value lives outside the repository (S4/S7).
    /// </summary>
    private const string SeedFixture = "fixture-not-a-secret";

    public DevDataSeederTests()
    {
        _hasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns<string>(p => $"hashed::{p}");

        // Deterministic identity: the real repository assigns ids; tests assign
        // sequential ones so the user->country wiring can be asserted.
        _countries
            .Setup(r => r.CreateAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()))
            .Callback<Country, CancellationToken>((c, _) => _createdCountries.Add(c))
            .Returns(Task.CompletedTask);

        _users
            .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => _createdUsers.Add(u))
            .Returns(Task.CompletedTask);

        _countries
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _createdCountries);
    }

    private DevDataSeeder CreateSeeder() =>
        new(_countries.Object, _users.Object, _hasher.Object, NullLogger<DevDataSeeder>.Instance);

    private static Country SeedCountry(string code, string name)
    {
        var country = Country.Create(code, name);
        typeof(Country).GetProperty(nameof(Country.Id))!.SetValue(country, 1);
        return country;
    }

    /// <summary>
    /// The seeded accounts must resolve against a country that actually exists
    /// in the database, so this setup returns one created country.
    /// </summary>
    private void GivenAnExistingCountry()
    {
        _createdCountries.Add(SeedCountry("CO", "Colombia"));
    }

    [Fact]
    public async Task SeedAsync_CreatesBaseCountries_WhenRepositoryIsEmpty()
    {
        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateSeeder().SeedAsync(SeedFixture);

        Assert.NotEmpty(_createdCountries);
        _countries.Verify(
            r => r.CreateAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SeedAsync_CreatesTheFourDocumentedAccounts_WithExpectedRoles()
    {
        GivenAnExistingCountry();
        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _users
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateSeeder().SeedAsync(SeedFixture);

        Assert.Equal(4, _createdUsers.Count);

        Assert.Equal("student@elearning.com", _createdUsers[0].Email);
        Assert.Equal(UserRole.Student, _createdUsers[0].Role);

        Assert.Equal("instructor@elearning.com", _createdUsers[1].Email);
        Assert.Equal(UserRole.Instructor, _createdUsers[1].Role);

        Assert.Equal("admin@elearning.com", _createdUsers[2].Email);
        Assert.Equal(UserRole.Admin, _createdUsers[2].Role);

        Assert.Equal("superadmin@elearning.com", _createdUsers[3].Email);
        Assert.Equal(UserRole.SuperAdmin, _createdUsers[3].Role);
    }

    [Fact]
    public async Task SeedAsync_SeededAccountsAreEmailVerified()
    {
        GivenAnExistingCountry();
        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _users
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateSeeder().SeedAsync(SeedFixture);

        Assert.Equal(4, _createdUsers.Count);
        Assert.All(_createdUsers, u => Assert.True(u.IsEmailVerified));
    }

    [Fact]
    public async Task SeedAsync_SeededAccountsBelongToAnExistingCountry()
    {
        GivenAnExistingCountry();
        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _users
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateSeeder().SeedAsync(SeedFixture);

        Assert.All(_createdUsers, u => Assert.Equal(1, u.CountryId));
    }

    [Fact]
    public async Task SeedAsync_HashesPasswordsWithThePasswordHasher_NeverStoringPlaintext()
    {
        GivenAnExistingCountry();
        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _users
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateSeeder().SeedAsync(SeedFixture);

        // Passwords must go through the hasher and never be stored as the
        // literal password value.
        Assert.All(_createdUsers, u => Assert.StartsWith("hashed::", u.PasswordHash));
        _hasher.Verify(h => h.Hash(SeedFixture), Times.Exactly(4));
    }

    [Fact]
    public async Task SeedAsync_WhenCountriesAndUsersAlreadyExist_DoesNotCreateAnything()
    {
        GivenAnExistingCountry();
        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _users
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSeeder().SeedAsync(SeedFixture);

        Assert.Single(_createdCountries);
        Assert.Empty(_createdUsers);
    }

    /// <summary>
    /// S7: without a configured dev seed password the seeder must fail closed.
    /// Credential-free countries are still written, but the users table must be
    /// left completely untouched and the hasher must never be called.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SeedAsync_WhenNoDevSeedPasswordIsConfigured_CreatesNoUserAndDoesNotThrow(string? configured)
    {
        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateSeeder().SeedAsync(configured);

        Assert.Equal(5, _createdCountries.Count);
        Assert.Empty(_createdUsers);
        _users.Verify(
            r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_SecondRunCreatesNothingNew()
    {
        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Simulate a repository that remembers what has already been created,
        // which is what makes a second run observably a no-op.
        _users
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string email, CancellationToken _) =>
                _createdUsers.Any(u => u.Email == email));

        _countries
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string code, CancellationToken _) =>
                _createdCountries.Any(c => c.Code == code));

        var seeder = CreateSeeder();

        await seeder.SeedAsync(SeedFixture);
        var countriesAfterFirstRun = _createdCountries.Count;
        var usersAfterFirstRun = _createdUsers.Count;

        await seeder.SeedAsync(SeedFixture);

        Assert.Equal(4, usersAfterFirstRun);
        Assert.Equal(countriesAfterFirstRun, _createdCountries.Count);
        Assert.Equal(usersAfterFirstRun, _createdUsers.Count);
    }
}
