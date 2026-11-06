import type { RouteItem } from '../../../core/api/routes'
import { formatDistance, formatDuration } from '../../../core/format'
import DataTable from '../../../components/ui/DataTable'
import { PencilIcon, TrashIcon } from '../../../components/ui/Icons'

interface RouteTableProps {
  routes: RouteItem[]
  loading?: boolean
  error?: string | null
  onEdit: (route: RouteItem) => void
  onRemove: (id: string) => void
}

export default function RouteTable({ routes, loading, error, onEdit, onRemove }: RouteTableProps) {
  return (
    <DataTable
      columns={['Nombre', 'Origen → Destino', 'Código', 'Tolerancia', 'Distancia', 'Vel. máx', 'Variantes', '']}
      loading={loading}
      error={error}
      rows={routes.length}
      empty="No hay rutas definidas."
    >
      {routes.map((r) => (
        <tr key={r.id}>
          <td>
            {r.name}
            {(r.alternatives ?? []).map((a) => (
              <div key={a.id} className="muted" style={{ fontSize: 12, lineHeight: 1.4 }}>
                ⤷ {a.name} · <span className="mono">{formatDistance(a.distanceKm)}</span>
              </div>
            ))}
          </td>
          <td>
            {r.originGeofenceName || r.destinationGeofenceName ? (
              <span className="muted">
                {r.originGeofenceName || '—'} → {r.destinationGeofenceName || '—'}
              </span>
            ) : (
              <span className="muted">solo trazado</span>
            )}
          </td>
          <td className="mono">{r.code}</td>
          <td className="mono">{r.toleranceM} m</td>
          <td className="mono">
            {formatDistance(r.distanceKm)}
            {r.estimatedDurationS > 0 && <span className="muted"> · {formatDuration(r.estimatedDurationS)}</span>}
          </td>
          <td className="mono">{r.maxSpeedKmh != null ? `${r.maxSpeedKmh} km/h` : '—'}</td>
          <td className="mono">
            {(r.alternatives?.length ?? 0) > 0 ? `${r.alternatives?.length ?? 0} alt.` : '—'}
          </td>
          <td style={{ textAlign: 'right' }}>
            <button className="btn" onClick={() => onEdit(r)} title="Editar">
              <PencilIcon size={14} />
            </button>{' '}
            <button className="btn danger" onClick={() => onRemove(r.id)}>
              <TrashIcon size={14} />
            </button>
          </td>
        </tr>
      ))}
    </DataTable>
  )
}