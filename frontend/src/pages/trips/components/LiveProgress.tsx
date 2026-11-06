import type { VehicleLive } from '../../../core/api/live'
import { formatDuration } from '../../../core/format'

interface LiveProgressProps {
  liveVehicle: VehicleLive
  progress: number | null
}

export default function LiveProgress({ liveVehicle, progress }: LiveProgressProps) {
  return (
    <section className="card" style={{ marginBottom: 16 }}>
      <div className="card-header">
        <h3>Avance en vivo</h3>
        <span className="mono">{formatDuration(liveVehicle.etaSec)} restantes</span>
      </div>
      <div className="card-body">
        <div style={{ display: 'flex', gap: 24, flexWrap: 'wrap', fontSize: 13 }}>
          <div><span className="d-label">Velocidad</span><span className="mono">{Math.round(liveVehicle.speedKmh ?? 0)} km/h</span></div>
          <div><span className="d-label">Carga</span><span className="mono">{Math.round(liveVehicle.loadTonnes ?? 0)} t de {liveVehicle.materialName ?? '—'}</span></div>
          <div><span className="d-label">ETA</span><span className="mono">{formatDuration(liveVehicle.etaSec)}</span></div>
        </div>
        <div className="progress-bar">
          <div style={{ width: `${progress ?? 0}%` }} />
        </div>
      </div>
    </section>
  )
}