import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'

// Consola limpia: se omite el aviso de React DevTools (necesita NODE_ENV=production
// para no mostrarse) y el error de un script externo (extensión) basado en
// 'web-vitals' que corre en un mundo aislado y no se puede interceptar por evento.
const silentConsoleLog = console.log.bind(console)
console.log = (...args: unknown[]) => {
  if (typeof args[0] === 'string' && args[0].includes('Download the React DevTools')) return
  silentConsoleLog(...args)
}
window.addEventListener(
  'error',
  (e) => {
    const msg = String(e.message ?? '')
    const info = String((e.error as Error | undefined)?.stack ?? '')
    if (msg.includes('reportAllChanges') || info.includes('reportAllChanges')) {
      e.preventDefault()
      e.stopImmediatePropagation()
    }
  },
  true,
)

createRoot(document.getElementById('root')!).render(<App />)