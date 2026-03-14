using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Users;

internal static class UserHelpers
{
    public static User BuildUser(
        Guid? id = null,
        string fullName = "Juan Pérez",
        string email = "juan@example.com",
        UserRole role = UserRole.Student,
        int countryId = 1,
        string countryName = "Colombia",
        bool verified = true)
    {
        var user = User.Create(fullName, email, "hash_bcrypt_xxx", countryId);

        Helpers.SetPrivate(user, "Id", id ?? Guid.NewGuid());
        Helpers.SetPrivate(user, "Role", role);
        Helpers.SetPrivate(user, "IsEmailVerified", verified);
        Helpers.SetPrivate(user, "CreatedAt", DateTime.UtcNow.AddDays(-30));

        // Inyectar Country navigation property
        var country = Country.Create(countryName.Length >= 3 ? countryName[..3].ToUpper() : "COL", countryName);
        Helpers.SetPrivate(country, "Id", countryId);
        Helpers.SetPrivate(user, "Country", country);

        return user;
    }

    public static Country BuildCountry(int id = 1, string code = "COL", string name = "Colombia", bool active = true)
    {
        var c = Country.Create(code, name);
        Helpers.SetPrivate(c, "Id", id);
        if (!active) c.Deactivate();
        return c;
    }

 
}
