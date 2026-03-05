import { api } from '../lib/axios';
import type { Country } from '../types';

export const countriesApi = {
  getActive: () =>
    api.get<CountryDto[]>('/countries'),
};