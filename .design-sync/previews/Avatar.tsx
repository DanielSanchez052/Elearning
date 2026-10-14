import { Avatar } from '@elearning/ui';

export const Sizes = () => (
  <div className="flex items-center gap-3">
    <Avatar name="Ana Gómez" size="xs" />
    <Avatar name="Ana Gómez" size="sm" />
    <Avatar name="Ana Gómez" size="md" />
    <Avatar name="Ana Gómez" size="lg" />
  </div>
);

export const Tones = () => (
  <div className="flex items-center gap-3">
    <Avatar name="Ana Gómez" size="md" />
    <Avatar name="Carlos Rivas" size="md" tone="neutral" />
  </div>
);

export const UserChip = () => (
  <div className="flex items-center gap-3">
    <Avatar name="Ana Gómez" />
    <div className="leading-tight">
      <p className="text-sm font-medium text-foreground">Ana Gómez</p>
      <p className="text-xs text-muted-foreground">Estudiante</p>
    </div>
  </div>
);
