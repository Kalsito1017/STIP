import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

const API_TARGET = process.env.API_TARGET || 'http://localhost:5000'
const TILE_TARGET = process.env.TILE_TARGET || 'http://localhost:8080'

export default defineConfig({
  envPrefix: ['VITE_'],
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  optimizeDeps: {
    include: ['leaflet', 'maplibre-gl'],
  },
  server: {
    port: 5173,
    proxy: {
      '/api': API_TARGET,
      '/hubs': {
        target: API_TARGET,
        ws: true,
      },
      '/tiles': {
        target: TILE_TARGET,
        changeOrigin: true,
      },
    },
  },
})
