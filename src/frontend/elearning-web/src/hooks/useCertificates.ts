import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { certificatesApi } from '@/api/certificates';
import { getApiErrorMessage } from '@/lib/axios';
import type { CertificateDto } from '@/types/certificate.types';

export const certificateKeys = {
  all: ['certificates'] as const,
  mine: () => ['certificates', 'me'] as const,
};

export function useMyCertificates(enabled = true) {
  return useQuery({
    queryKey: certificateKeys.mine(),
    queryFn: () => certificatesApi.getMyCertificates().then((r) => r.data),
    enabled,
    staleTime: 1000 * 60,
  });
}

/** Descarga el PDF de un certificado (endpoint autenticado → blob → enlace temporal). */
export function useDownloadCertificate() {
  const [downloadingId, setDownloadingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const download = async (certificate: CertificateDto) => {
    setError(null);
    setDownloadingId(certificate.id);
    try {
      const response = await certificatesApi.downloadCertificate(certificate.certificateUrl);
      const blobUrl = window.URL.createObjectURL(response.data);
      const link = document.createElement('a');
      link.href = blobUrl;
      link.download = `certificado-${certificate.courseName}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(blobUrl);
    } catch (e) {
      setError(getApiErrorMessage(e));
    } finally {
      setDownloadingId(null);
    }
  };

  return { download, downloadingId, error };
}
