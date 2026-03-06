import axios from '@/lib/axios';
import type { Country } from '@/types/user.types';

export const countriesApi = {
  getCountries: () =>
    axios.get<Country[]>('/admin/countries'),

  getCountryById: (id: number) =>
    axios.get<Country>(`/admin/countries/${id}`),

  createCountry: (data: { code: string; name: string }) =>
    axios.post<number>('/admin/countries', data),

  toggleCountryStatus: (id: number) =>
    axios.patch(`/admin/countries/${id}/toggle-status`),
};
