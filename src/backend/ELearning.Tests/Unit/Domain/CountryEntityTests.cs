using ELearning.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Domain;

public class CountryEntityTests
{
    [Fact]
    public void Create_NewCountry_IsActiveByDefault()
    {
        var country = Country.Create("COL", "Colombia");

        Assert.True(country.IsActive);
    }

    [Fact]
    public void Create_NewCountry_HasCorrectCodeAndName()
    {
        var country = Country.Create("MEX", "México");

        Assert.Equal("MEX", country.Code);
        Assert.Equal("México", country.Name);
    }

    [Fact]
    public void Deactivate_ActiveCountry_SetsIsActiveFalse()
    {
        var country = Country.Create("COL", "Colombia");

        country.Deactivate();

        Assert.False(country.IsActive);
    }

    [Fact]
    public void Activate_InactiveCountry_SetsIsActiveTrue()
    {
        var country = Country.Create("COL", "Colombia");
        country.Deactivate();

        country.Activate();

        Assert.True(country.IsActive);
    }

    [Fact]
    public void UpdateName_ChangesNameCorrectly()
    {
        var country = Country.Create("COL", "Colombia");

        country.UpdateName("República de Colombia");

        Assert.Equal("República de Colombia", country.Name);
    }

    [Fact]
    public void UpdateCode_ChangesCodeCorrectly()
    {
        var country = Country.Create("COL", "Colombia");

        country.UpdateCode("CBL");

        Assert.Equal("CBL", country.Code);
    }
}
