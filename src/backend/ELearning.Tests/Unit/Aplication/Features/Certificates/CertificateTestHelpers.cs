using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Certificates;

internal static class CertificateTestHelpers
{
    public static readonly Guid UserId = Guid.NewGuid();
    public static readonly Guid CourseId = Guid.NewGuid();

    /// <summary>
    /// Inscripción en estado Completed con CompletedAt asignado.
    /// Incluye Course.Title navegado vía reflexión.
    /// </summary>
    public static CourseEnrollment BuildCompletedEnrollment(
        Guid? userId = null,
        Guid? courseId = null,
        string courseTitle = "Curso de prueba")
    {
        var uid = userId ?? UserId;
        var cid = courseId ?? CourseId;

        var enrollment = CourseEnrollment.Create(uid, cid);

        // Forzar Status = Completed y CompletedAt vía reflexión
        SetPrivate(enrollment, "Status", EnrollmentStatus.Completed);
        SetPrivate(enrollment, "CompletedAt", DateTime.UtcNow.AddDays(-1));
        SetPrivate(enrollment, "Id", Guid.NewGuid());

        // Inyectar Course navegado
        var course = Course.Create(
            title: courseTitle,
            description: null,
            thumbnailUrl: null,
            createdBy: Guid.NewGuid(),
            isGlobal: true);

        SetPrivate(enrollment, "Course", course);
        SetPrivate(enrollment, "CourseId", cid);

        return enrollment;
    }

    /// <summary>Inscripción activa (sin completar).</summary>
    public static CourseEnrollment BuildActiveEnrollment(Guid? userId = null, Guid? courseId = null)
    {
        var enrollment = CourseEnrollment.Create(userId ?? UserId, courseId ?? CourseId);
        SetPrivate(enrollment, "Id", Guid.NewGuid());
        return enrollment;
    }

    public static User BuildUser(string name = "Ana García")
    {
        var user = User.Create(name, "ana@test.com", "hash", countryId: 1);
        return user;
    }

    public static void SetPrivate(object obj, string propertyName, object? value)
    {
        var prop = obj.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(obj, value);
    }
}
