import { useState } from 'react';
import { useAuthStore } from '../store/authStore';
import { useMyEnrollments } from '@/hooks/useEnrollments';
import { useMyCertificates } from '@/hooks/useCertificates';
import { certificatesApi } from '@/api/certificates';
import { getApiErrorMessage } from '@/lib/axios';
import type { CertificateDto } from '@/types/certificate.types';

// El status de la inscripción puede llegar como código numérico o como string,
// según la config del serializador JSON del backend (mismo criterio que
// `getEnrollmentStatusLabel` en CourseDetailPage.tsx).
function isCompletedStatus(status: unknown) {
  return status === 1 || status === 'Completed';
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString();
}

export const ProfilePage = () => {
  const { user } = useAuthStore();
  const enrollmentsQuery = useMyEnrollments();
  const certificatesQuery = useMyCertificates();
  const [downloadingId, setDownloadingId] = useState<string | null>(null);
  const [downloadError, setDownloadError] = useState<string | null>(null);

  const completedEnrollments = (enrollmentsQuery.data ?? []).filter((e) =>
    isCompletedStatus(e.status)
  );

  const handleDownload = async (certificate: CertificateDto) => {
    setDownloadError(null);
    setDownloadingId(certificate.id);
    try {
      const response = await certificatesApi.downloadCertificate(
        certificate.certificateUrl
      );
      const blobUrl = window.URL.createObjectURL(response.data);
      const link = document.createElement('a');
      link.href = blobUrl;
      link.download = `certificado-${certificate.courseName}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(blobUrl);
    } catch (error) {
      setDownloadError(getApiErrorMessage(error));
    } finally {
      setDownloadingId(null);
    }
  };

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-4">Mi Perfil</h1>

      <div className="max-w-2xl bg-white rounded shadow p-6 mb-8">
        <h2 className="text-2xl font-bold mb-4">Información Personal</h2>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <p className="text-gray-600">Nombre</p>
            <p className="font-bold">{user?.fullName}</p>
          </div>
          <div>
            <p className="text-gray-600">Email</p>
            <p className="font-bold">{user?.email}</p>
          </div>
          <div>
            <p className="text-gray-600">Rol</p>
            <p className="font-bold">{user?.role}</p>
          </div>
        </div>
      </div>

      <div className="max-w-2xl bg-white rounded shadow p-6 mb-8">
        <h2 className="text-xl font-bold mb-4">Cursos completados</h2>
        {enrollmentsQuery.isLoading ? (
          <p className="text-gray-600">Cargando cursos...</p>
        ) : enrollmentsQuery.isError ? (
          <p className="text-gray-600">
            No se pudieron cargar tus cursos completados.
          </p>
        ) : completedEnrollments.length === 0 ? (
          <p className="text-gray-600">No hay cursos completados aún</p>
        ) : (
          <ul className="divide-y">
            {completedEnrollments.map((enrollment) => (
              <li
                key={enrollment.enrollmentId}
                className="py-3 flex items-center gap-4"
              >
                <div className="w-14 h-14 rounded bg-gray-100 overflow-hidden flex-shrink-0">
                  {enrollment.courseThumbnailUrl && (
                    <img
                      src={enrollment.courseThumbnailUrl}
                      alt={enrollment.courseTitle}
                      className="w-full h-full object-cover"
                    />
                  )}
                </div>
                <div>
                  <p className="font-bold">{enrollment.courseTitle}</p>
                  {enrollment.completedAt && (
                    <p className="text-gray-600 text-sm">
                      Completado el {formatDate(enrollment.completedAt)}
                    </p>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="bg-white rounded shadow p-6">
          <h2 className="text-xl font-bold mb-4">Badges</h2>
          <p className="text-gray-600">No hay badges aún</p>
        </div>
        <div className="bg-white rounded shadow p-6">
          <h2 className="text-xl font-bold mb-4">Certificados</h2>
          {certificatesQuery.isLoading ? (
            <p className="text-gray-600">Cargando certificados...</p>
          ) : certificatesQuery.isError ? (
            <p className="text-gray-600">
              No se pudieron cargar tus certificados.
            </p>
          ) : (certificatesQuery.data ?? []).length === 0 ? (
            <p className="text-gray-600">No hay certificados aún</p>
          ) : (
            <ul className="divide-y">
              {(certificatesQuery.data ?? []).map((certificate) => (
                <li key={certificate.id} className="py-3">
                  <p className="font-bold">{certificate.courseName}</p>
                  <p className="text-gray-600 text-sm">
                    Completado el {formatDate(certificate.completedAt)}
                  </p>
                  <button
                    type="button"
                    onClick={() => handleDownload(certificate)}
                    disabled={downloadingId === certificate.id}
                    className="mt-2 text-sm font-medium text-indigo-600 hover:text-indigo-500 disabled:opacity-60"
                  >
                    {downloadingId === certificate.id
                      ? 'Descargando...'
                      : 'Descargar certificado'}
                  </button>
                </li>
              ))}
            </ul>
          )}
          {downloadError && (
            <p className="text-red-600 text-sm mt-2">{downloadError}</p>
          )}
        </div>
      </div>
    </div>
  );
};
