using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Certificates.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Certificates;

public class GetCertificateByCourseHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly GetCertificateByCourseHandler _handler;

    public GetCertificateByCourseHandlerTests() =>
        _handler = new GetCertificateByCourseHandler(_enrollmentsMock.Object);

    [Fact]
    public async Task HandleAsync_CompletedEnrollment_ReturnsCertificateDto()
    {
        var enrollment = CertificateTestHelpers.BuildCompletedEnrollment(courseTitle: "Curso Test");
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(
            new GetCertificateByCourseQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.True(result.IsSuccess);
        Assert.Equal("Curso Test", result.Value.CourseName);
        Assert.Equal(enrollment.Id, result.Value.Id);
        Assert.NotEqual(Guid.Empty, result.Value.CourseId);
    }

    [Fact]
    public async Task HandleAsync_NoEnrollment_ReturnsNotFound()
    {
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId, default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(
            new GetCertificateByCourseQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ActiveEnrollment_ReturnsConflict()
    {
        var enrollment = CertificateTestHelpers.BuildActiveEnrollment();
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(
            new GetCertificateByCourseQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
        Assert.Contains("completar", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(
            new GetCertificateByCourseQuery(Guid.Empty, CertificateTestHelpers.CourseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_EmptyCourseId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(
            new GetCertificateByCourseQuery(CertificateTestHelpers.UserId, Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_CompletedEnrollment_CertificateUrlContainsCourseId()
    {
        var enrollment = CertificateTestHelpers.BuildCompletedEnrollment();
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(
            new GetCertificateByCourseQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.Contains(enrollment.CourseId.ToString(), result.Value.CertificateUrl);
        Assert.EndsWith("/download", result.Value.CertificateUrl);
    }
}
