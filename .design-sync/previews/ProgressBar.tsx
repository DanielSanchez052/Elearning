import { ProgressBar } from '@elearning/ui';

export const CourseProgress = () => (
  <div className="w-96">
    <ProgressBar value={75} label="3 / 4 lecciones requeridas" showValue tone="success" />
  </div>
);

export const Upload = () => (
  <div className="w-96">
    <ProgressBar value={42} label="Subiendo introduccion.mp4" showValue size="sm" />
  </div>
);

export const Tones = () => (
  <div className="flex w-96 flex-col gap-4">
    <ProgressBar value={30} tone="brand" />
    <ProgressBar value={55} tone="success" />
    <ProgressBar value={80} tone="warning" />
    <ProgressBar value={40} tone="danger" />
  </div>
);
