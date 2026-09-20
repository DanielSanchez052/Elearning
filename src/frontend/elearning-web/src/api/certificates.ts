import axios from '@/lib/axios';
import type { CertificateDto } from '@/types/certificate.types';

export const certificatesApi = {
  getMyCertificates: () => axios.get<CertificateDto[]>('/certificates/me'),

  // El DTO trae `certificateUrl` con el prefijo `/api/...` (ruta absoluta del
  // backend), pero nuestro axios ya tiene `baseURL` = `<host>/api`. Si se pasara
  // tal cual, axios duplicaría el segmento `/api`. Se quita el prefijo antes de
  // combinarlo con baseURL para que resuelva a la misma URL que expone el DTO.
  downloadCertificate: (certificateUrl: string) =>
    axios.get<Blob>(certificateUrl.replace(/^\/api(?=\/)/, ''), {
      responseType: 'blob',
    }),
};
