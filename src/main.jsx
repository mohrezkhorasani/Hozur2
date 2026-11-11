import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import { initGlobalErrorLogging } from './Tools/telegramLogger'

// Initialize global exception and network error logging to Telegram
initGlobalErrorLogging()

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
