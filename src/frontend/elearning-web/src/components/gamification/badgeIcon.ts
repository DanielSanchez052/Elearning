import { AwardIcon, MedalIcon, TrophyIcon, ZapIcon } from '@/components/ui/icons';

/** Ícono por código de medalla (LoginFirst, CourseDone, Speedster); AwardIcon para cualquier otra. */
export function badgeIcon(code: string) {
  switch (code) {
    case 'LoginFirst':
      return MedalIcon;
    case 'CourseDone':
      return TrophyIcon;
    case 'Speedster':
      return ZapIcon;
    default:
      return AwardIcon;
  }
}
