using ELearning.Application.Common.Authorization;

namespace ELearning.Tests.Unit.Aplication.Common;

public class ReportAccessTests
{
    [Theory]
    [InlineData("admin", true)]
    [InlineData("superadmin", true)]
    [InlineData("instructor", false)]
    [InlineData("student", false)]
    [InlineData("1", false)]
    public void CanViewPlatformReports_OnlyAdminsAndSuperAdmins(string role, bool expected) =>
        Assert.Equal(expected, ReportAccess.CanViewPlatformReports(role));

    [Theory]
    [InlineData(null, null)]   // todos los países
    [InlineData(52, 52)]       // el país que pida
    public void ResolveCountry_SuperAdminChooses(int? requested, int? expected) =>
        Assert.Equal(expected, ReportAccess.ResolveCountry("superadmin", 57, requested));

    [Theory]
    [InlineData(null)]
    [InlineData(52)]           // aunque pida otro país
    public void ResolveCountry_AdminIsAlwaysScopedToOwnCountry(int? requested) =>
        Assert.Equal(57, ReportAccess.ResolveCountry("admin", 57, requested));
}
