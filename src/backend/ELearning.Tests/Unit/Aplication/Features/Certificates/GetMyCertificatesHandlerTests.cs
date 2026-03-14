using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Certificates.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Certificates;

public class GetMyCertificatesHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly GetMyCertificatesHandler _handler;

    public GetMyCertificatesHandlerTests() =>
        _handler = new GetMyCertificatesHandler(_enrollmentsMock.Object);

    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserWithCompletedEnrollments_ReturnsCertificateList()
    {
        var e1 = CertificateTestHelpers.BuildCompletedEnrollment(courseTitle: "Curso A");
        var e2 = CertificateTestHelpers.BuildCompletedEnrollment(courseTitle: "Curso B");
        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync(new[] { e1, e2 });

        var result = await _handler.HandleAsync(
            new GetMyCertificatesQuery(CertificateTestHelpers.UserId));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task HandleAsync_UserWithNoCompletedEnrollments_ReturnsEmptyList()
    {
        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync(new[] { CertificateTestHelpers.BuildActiveEnrollment() });

        var result = await _handler.HandleAsync(
            new GetMyCertificatesQuery(CertificateTestHelpers.UserId));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_UserWithNoEnrollments_ReturnsEmptyList()
    {
        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync(Array.Empty<CourseEnrollment>());

        var result = await _handler.HandleAsync(
            new GetMyCertificatesQuery(CertificateTestHelpers.UserId));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_MixedEnrollments_ReturnsOnlyCompleted()
    {
        var completed = CertificateTestHelpers.BuildCompletedEnrollment(courseTitle: "Terminado");
        var active = CertificateTestHelpers.BuildActiveEnrollment();
        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync(new[] { completed, active });

        var result = await _handler.HandleAsync(
            new GetMyCertificatesQuery(CertificateTestHelpers.UserId));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Terminado", result.Value[0].CourseName);
    }

    [Fact]
    public async Task HandleAsync_CompletedEnrollments_AreMappedCorrectly()
    {
        var enrollment = CertificateTestHelpers.BuildCompletedEnrollment(courseTitle: "Mi Curso");
        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync(new[] { enrollment });

        var result = await _handler.HandleAsync(
            new GetMyCertificatesQuery(CertificateTestHelpers.UserId));

        var cert = result.Value[0];
        Assert.Equal("Mi Curso", cert.CourseName);
        Assert.Equal(enrollment.Id, cert.Id);
        Assert.Equal(enrollment.CourseId, cert.CourseId);
        Assert.Equal(enrollment.CompletedAt!.Value, cert.CompletedAt);
        Assert.Contains("/api/certificates/courses/", cert.CertificateUrl);
        Assert.Contains(enrollment.CourseId.ToString(), cert.CertificateUrl);
    }

    [Fact]
    public async Task HandleAsync_CertificateUrl_PointsToDownloadEndpoint()
    {
        var enrollment = CertificateTestHelpers.BuildCompletedEnrollment();
        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync(new[] { enrollment });

        var result = await _handler.HandleAsync(
            new GetMyCertificatesQuery(CertificateTestHelpers.UserId));

        var url = result.Value[0].CertificateUrl;
        Assert.EndsWith("/download", url);
    }

    [Fact]
    public async Task HandleAsync_MultipleCompleted_OrderedByCompletedAtDescending()
    {
        var older = CertificateTestHelpers.BuildCompletedEnrollment(courseTitle: "Primero");
        CertificateTestHelpers.SetPrivate(older, "CompletedAt", DateTime.UtcNow.AddDays(-10));

        var newer = CertificateTestHelpers.BuildCompletedEnrollment(courseTitle: "Último");
        CertificateTestHelpers.SetPrivate(newer, "CompletedAt", DateTime.UtcNow.AddDays(-1));

        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync(new[] { older, newer }); // orden invertido

        var result = await _handler.HandleAsync(
            new GetMyCertificatesQuery(CertificateTestHelpers.UserId));

        Assert.Equal("Último", result.Value[0].CourseName);
        Assert.Equal("Primero", result.Value[1].CourseName);
    }

    // ── Validación de entrada ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetMyCertificatesQuery(Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }
}
