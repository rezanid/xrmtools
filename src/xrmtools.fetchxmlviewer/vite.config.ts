import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    base: './',
    server: {
        port: 56944,
        strictPort: true
    },
    build: {
        outDir: 'dist',
        emptyOutDir: true,
        target: 'es2019',
        sourcemap: true,
    }
})
