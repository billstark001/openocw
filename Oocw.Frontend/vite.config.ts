import { defineConfig } from 'vite';
import * as path from 'path';

import vue from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    // mkcert(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  },
  server: {
    port: 5040,
    proxy: {
      '/api': {
        target: 'http://localhost:5051',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, '/api')
      }
    },
  },
  // SSR build configuration
  // Run `vite build --ssr src/entry-server.ts` to produce the server bundle.
  // Run `vite build` to produce the client bundle (used for hydration).
  ssr: {
    // Inline all dependencies into the server bundle for simpler deployment.
    noExternal: /./,
  },
});
