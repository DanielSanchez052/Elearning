import { Textarea } from '@elearning/ui';

export const Default = () => (
  <div className="w-96">
    <Textarea rows={3} placeholder="Describe de qué trata el curso..." />
  </div>
);

export const Filled = () => (
  <div className="w-96">
    <Textarea
      rows={4}
      defaultValue="Aprende tablas dinámicas, funciones de búsqueda y automatización con macros para analizar datos de forma profesional."
    />
  </div>
);
