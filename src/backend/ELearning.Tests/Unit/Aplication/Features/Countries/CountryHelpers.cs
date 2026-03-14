using ELearning.Domain.Entities;

namespace ELearning.Tests.Unit.Aplication.Features.Countries;

internal static class CountryHelpers
{
    public static Country BuildActive(int id = 1, string code = "COL", string name = "Colombia")
    {
        var c = Country.Create(code, name); // IsActive = true por defecto
        Helpers.SetPrivate(c, "Id", id);
        return c;
    }

    public static Country BuildInactive(int id = 2, string code = "ARG", string name = "Argentina")
    {
        var c = Country.Create(code, name);
        Helpers.SetPrivate(c, "Id", id);
        c.Deactivate();
        return c;
    }

    
}
