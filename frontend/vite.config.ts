import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const API_TARGET = process.env.API_TARGET || 'http://localhost:5000'

export default defineConfig({
  envPrefix: ['VITE_', 'MAPBOX_'],
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      '/api': API_TARGET,
      '/hubs': {
        target: API_TARGET,
        ws: true,
      },
    },
  },
})
