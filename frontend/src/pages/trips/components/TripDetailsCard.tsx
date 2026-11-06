import type { TripDetail } from '../../../core/api/trips'
import { formatDateTime } from '../../../core/format'

const STATUS_BADGE: Record<string, string> = {
  Active: 'green',
  Completed: 'blue',
  Cancelled: 'gray',
}

interface TripDetailsCardProps {
  detail: TripDetail
}

export default function TripDetailsCard({ detail }: TripDetailsCardProps) {
  return (
    <div className="card">
      <div className="card-header"><h3>Detalle</h3></div>
      <div className="card-body" style={{ fontSize: 13 }}>
        <div className="detail-grid">
          <div><span className="d-label">Unidad</span><span className="mono">{detail.vehicleCode}</span></div>
          <div><span className="d-label">Origen</span><span>{detail.originName ?? '—'}</span></div>
          <div><span className="d-label">Destino</span><span>{detail.destinationName ?? '—'}</span></div>
          <div><span className="d-label">Ruta</span><span>{detail.routeName ?? detail.routeCode ?? '—'}</span></div>
          <div><span className="d-label">Inicio</span><span className="mono">{formatDateTime(detail.startedAt)}</span></div>
          <div><span className="d-label">Fin</span><span className="mono">{formatDateTime(detail.endedAt)}</span></div>
          <div><span className="d-label">Alertas de velocidad</span><span className="mono">{detail.speedAlertCount}</span></div>
          <div><span className="d-label">Estado</span><span className={`badge ${STATUS_BADGE[detail.status] ?? 'gray'}`}>{detail.status}</span></div>
        </div>
      </div>
    </div>
  )
}