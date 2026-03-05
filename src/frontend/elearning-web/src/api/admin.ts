import axios from '../lib/axios';

export const adminApi = {
  getUsers: (filters?: any) =>
    axios.get('/admin/users', { params: filters }),
  
  createCountry: (name: string, code: string) =>
    axios.post('/admin/countries', { name, code }),
  
  assignAdminToCountry: (userId: string, countryId: number) =>
    axios.post('/admin/assign-country', { userId, countryId }),
  
  getCountryStats: (countryId: number) =>
    axios.get(`/admin/countries/${countryId}/stats`),
  
  getUserProgressReport: () =>
    axios.get('/reports/user-progress'),
  
  getCountryReport: (countryId: number) =>
    axios.get(`/reports/country/${countryId}`),
  
  getLeaderboard: () =>
    axios.get('/reports/leaderboard'),
};
