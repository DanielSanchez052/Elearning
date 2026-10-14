using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Seeding;

/// <summary>
/// Seeds the minimum development data needed to exercise the API from a fresh
/// database: the base country set and the four documented dev accounts.
///
/// A freshly migrated database contains only the 3 badge rows. Without users
/// and countries the API is unusable end to end: registration requires an
/// existing country, and country creation requires a superadmin that cannot be
/// created without a country. This breaks that circular dependency.
///
/// Development data only. Callers must guard by environment; see
/// <c>Program.cs</c>, which invokes this exclusively under Development. The
/// shared account password is never stored here: it is supplied through
/// configuration and an unset key skips the accounts instead of inventing one.
/// </summary>
public class DevDataSeeder
{
    /// <summary>
    /// Host environment name that enables this seeder. Matches
    /// <c>Microsoft.Extensions.Hosting.Environments.Development</c> without
    /// taking a Hosting dependency in the Infrastructure layer.
    /// </summary>
    public const string DevelopmentEnvironmentName = "Development";

    /// <summary>
    /// Configuration key that supplies the password shared by the seeded dev
    /// accounts. There is deliberately no default value: a credential literal
    /// must never be committed, so an unset key makes the seeder skip the
    /// accounts instead of inventing a well-known one.
    /// </summary>
    public const string DevSeedPasswordKey = "DevSeed:Password";

    private static readonly (string Code, string Name)[] BaseCountries =
    {
        ("CO", "Colombia"),
        ("MX", "México"),
        ("AR", "Argentina"),
        ("PE", "Perú"),
        ("CL", "Chile"),
    };

    private static readonly (string FullName, string Email, UserRole Role)[] DevAccounts =
    {
        ("Estudiante Demo", "student@elearning.com", UserRole.Student),
        ("Instructor Demo", "instructor@elearning.com", UserRole.Instructor),
        ("Admin Demo", "admin@elearning.com", UserRole.Admin),
        ("Super Admin Demo", "superadmin@elearning.com", UserRole.SuperAdmin),
    };

    private readonly ICountryRepository _countries;
    private readonly IUserRepository _users;
    private readonly IPasswordHasherService _hasher;
    private readonly ILogger<DevDataSeeder> _logger;

    public DevDataSeeder(
        ICountryRepository countries,
        IUserRepository users,
        IPasswordHasherService hasher,
        ILogger<DevDataSeeder> logger)
    {
        _countries = countries;
        _users = users;
        _hasher = hasher;
        _logger = logger;
    }

    /// <summary>
    /// Creates any missing base country and dev account. Idempotent: existing
    /// rows are never duplicated and never overwritten.
    /// </summary>
    /// <param name="devSeedPassword">
    /// Password shared by the seeded accounts, read from
    /// <see cref="DevSeedPasswordKey"/>. When it is missing or blank, no user
    /// row is written; countries are still seeded.
    /// </param>
    public virtual async Task SeedAsync(string? devSeedPassword, CancellationToken ct = default)
    {
        await SeedCountriesAsync(ct);
        await SeedAccountsAsync(devSeedPassword, ct);
    }

    /// <summary>
    /// Runs <see cref="SeedAsync"/> only when the supplied host environment is
    /// Development.
    ///
    /// This is the security boundary for the seeder: it creates shared demo
    /// accounts with a well-known password, so it must never execute against a
    /// non-development database. Exposed as a method taking the environment
    /// name so the boundary is directly testable.
    /// </summary>
    public virtual async Task SeedForEnvironmentAsync(
        string environmentName,
        string? devSeedPassword,
        CancellationToken ct = default)
    {
        if (!string.Equals(environmentName, DevelopmentEnvironmentName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await SeedAsync(devSeedPassword, ct);
    }

    private async Task SeedCountriesAsync(CancellationToken ct)
    {
        foreach (var (code, name) in BaseCountries)
        {
            if (await _countries.ExistsByCodeAsync(code, ct))
            {
                continue;
            }

            await _countries.CreateAsync(Country.Create(code, name), ct);
            _logger.LogInformation("Seeded country {Code} ({Name})", code, name);
        }
    }

    private async Task SeedAccountsAsync(string? devSeedPassword, CancellationToken ct)
    {
        // Fail closed. No configured password means no shared demo account,
        // never a default one: the accounts are only usable in a local
        // Development database anyway.
        if (string.IsNullOrWhiteSpace(devSeedPassword))
        {
            _logger.LogWarning(
                "{Key} is not configured; skipping dev account seeding. Set it with: " +
                "dotnet user-secrets set \"{Key}\" \"<password>\" --project ELearning.API",
                DevSeedPasswordKey,
                DevSeedPasswordKey);
            return;
        }

        var countries = await _countries.GetAllAsync(ct);

        if (countries.Count == 0)
        {
            _logger.LogWarning("No countries available; skipping dev account seeding.");
            return;
        }

        // Country ids are database-generated, so resolve the seeded country
        // instead of hardcoding an id.
        var countryId = countries[0].Id;

        foreach (var (fullName, email, role) in DevAccounts)
        {
            if (await _users.ExistsByEmailAsync(email, ct))
            {
                continue;
            }

            var user = User.Create(fullName, email, _hasher.Hash(devSeedPassword), countryId, role);

            // Seeded accounts are immediately usable: they must not be blocked
            // behind the email-verification step.
            user.VerifyEmail();

            await _users.CreateAsync(user, ct);
            _logger.LogInformation("Seeded dev account {Email} ({Role})", email, role);
        }
    }
}
