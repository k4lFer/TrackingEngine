import { useLocation } from 'react-router-dom'
import { useMapBase, BASE_MAP_LABELS } from '../BaseMap'
import type { BaseMapKind } from '../BaseMap'
import { useSignalR } from '../../core/hooks/useSignalR'
import { BellIcon } from '../ui/Icons'

const TITLES: Record<string, string> = {
  '/': 'Panel de monitoreo',
  '/fleet': 'Flota',
  '/trips': 'Viajes',
  '/events': 'Eventos',
  '/geofences': 'Geocercas',
  '/routes': 'Rutas',
}

export default function Topbar() {
  const location = useLocation()
  const title = TITLES[location.pathname] ?? 'Panel de monitoreo'
  const { kind, setKind } = useMapBase()
  const { connected } = useSignalR()

  return (
    <header className="flex items-center justify-between px-6 py-3.5 border-b border-line bg-surface max-sm:px-4">
      <h1 className="font-display text-base font-semibold">{title}</h1>
      <div className="flex items-center gap-3 font-mono text-xs text-ink-dim">
        <label className="flex items-center gap-1.5 text-xs text-ink-dim">
          <span>Mapa:</span>
          <select
            value={kind}
            onChange={(e) => setKind(e.target.value as BaseMapKind)}
            className="bg-surface-2 text-ink border border-line-strong rounded-[6px] px-2 py-1 text-xs font-mono cursor-pointer"
          >
            {(Object.keys(BASE_MAP_LABELS) as BaseMapKind[]).map((k) => (
              <option key={k} value={k}>
                {BASE_MAP_LABELS[k]}
              </option>
            ))}
          </select>
        </label>
        <span className={`w-2 h-2 rounded-full ${connected ? 'bg-ok' : 'bg-ink-faint'}`} />
        <span className="mono">{connected ? 'Conectado' : 'Sin conexión'}</span>
        <BellIcon size={16} />
      </div>
    </header>
  )
}