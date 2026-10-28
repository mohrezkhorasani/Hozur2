import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'   // ← این خط مهمه
// https://vite.dev/config/
export default defineConfig({

  server: {
    allowedHosts: [
      'localhost',
      '127.0.0.1',
      'vc5.hekmateislami.com'
    ],
    host: true, // This will listen on all addresses
    port: 3000, // You can specify the port here too
    historyApiFallback: true,
  },
  plugins: [
    react(),
    tailwindcss(),
  ],
  build: {
    target: ['chrome80', 'firefox78', 'edge80', 'safari13'],
    cssTarget: ['chrome80', 'firefox78', 'edge80', 'safari13'],
  },
  resolve: {
    alias: {
      // گزینه ۱: @ یعنی src (رایج‌ترین و بهترین)
      '@': path.resolve(__dirname, './src'),

      // گزینه ۲: اگر حتما می‌خوای با src/ شروع بشه (کمتر رایج ولی ممکنه)
      // 'src': path.resolve(__dirname, './src'),

      // گزینه ۳: aliasهای اضافی (اختیاری ولی خیلی مفید)
      '@assets': path.resolve(__dirname, './src/assets'),
      '@components': path.resolve(__dirname, './src/components'),
      '@js': path.resolve(__dirname, './src/js'),
    }
  }
})
