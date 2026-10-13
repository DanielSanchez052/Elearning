// Entrada del build de librería del sistema de diseño (vite.ds.config.ts → dist-ds/).
// La app NO importa este archivo: usa '@/components/ui' y carga index.css desde main.tsx.
import './ds-library.css';

export * from './index';
