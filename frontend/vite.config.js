import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ command, mode }) => {
  const env = loadEnv(mode, process.cwd(), 'VITE_')

  if (command === 'build' && mode === 'production' && !env.VITE_API_URL) {
    throw new Error('Falta la variable de entorno VITE_API_URL con la URL pública de la API.')
  }

  return {
    plugins: [react()],
  }
})
