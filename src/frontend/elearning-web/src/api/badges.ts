import axios from '@/lib/axios';
import type { UserBadgeDto } from '@/types/badge.types';

export const badgesApi = {
  getMyBadges: () => axios.get<UserBadgeDto[]>('/badges/me'),
};
