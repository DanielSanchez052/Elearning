import path from "path"
import { writeFileSync } from 'fs'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// Build de librería del sistema de diseño (src/components/ui → dist-ds/).
// Lo consume la sincronización con Claude Design (.design-sync/); no afecta al build de la app (vite.config.ts).
// Uso: npm run build:ds
const outDir = path.resolve(__dirname, 'dist-ds')

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    {
      // dist-ds/ se trata como un paquete: entrada JS, tipos y hoja de estilos.
      name: 'ds-package-json',
      writeBundle() {
        writeFileSync(
          path.join(outDir, 'package.json'),
          JSON.stringify(
            {
              name: '@elearning/ui',
              version: '0.0.0',
              private: true,
              type: 'module',
              module: './index.js',
              types: './types/components/ui/index.d.ts',
              style: './styles.css',
              sideEffects: ['*.css'],
            },
            null,
            2
          ) + '\n'
        )
      },
    },
  ],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    outDir,
    emptyOutDir: true,
    copyPublicDir: false,
    lib: {
      entry: path.resolve(__dirname, 'src/components/ui/ds-library.ts'),
      formats: ['es'],
      fileName: () => 'index.js',
      cssFileName: 'styles',
    },
    rollupOptions: {
      external: ['react', 'react-dom', 'react/jsx-runtime'],
    },
  },
})
