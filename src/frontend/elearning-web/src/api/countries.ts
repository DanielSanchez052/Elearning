import axios from '@/lib/axios';
import type { Country } from '../types/user.types';

export const countriesApi = {
  getActive: () =>
    axios.get<Country[]>('/countries'),
};