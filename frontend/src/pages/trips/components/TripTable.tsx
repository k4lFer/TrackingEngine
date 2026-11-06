import type { TripSummary } from '../../../core/api/trips'
import { formatDateTime, formatDistance } from '../../../core/format'
import DataTable from '../../../components/ui/DataTable'
import { CloseIcon, PencilIcon } from '../../../components/ui/Icons'

const STATUS_BADGE: Record<string, string> = {
  Active: 'green',
  Completed: 'blue',
  Cancelled: 'gray',
  Incomplete: 'amber',
}

const STATUS_LABELS: Record<string, string> = {
  Active: 'En curso',
  Completed: 'Completado',
  Cancelled: 'Cancelado',
  Incomplete: 'Incompleto',
}

interface TripTableProps {
  trips: TripSummary[]
  loading?: boolean
  error?: string | null
  empty?: string
  onOpen: (id: string) => void
  onEdit: (trip: TripSummary) => void
  onCancel: (trip: TripSummary) => void
}

export default function TripTable({ trips, loading, error, empty, onOpen, onEdit, onCancel }: TripTableProps) {
  return (
    <DataTable
      columns={['Unidad', 'Origen → Destino', 'Material', 'Carga', 'Inicio', 'Distancia', 'Estado', '']}
      loading={loading}
      error={error}
      rows={trips.length}
      empty={empty ?? 'No hay viajes registrados.'}
    >
      {trips.map((t) => (
        <tr key={t.id} onClick={() => onOpen(t.id)} style={{ cursor: 'pointer' }}>
          <td className="mono">{t.vehicleCode}</td>
          <td>
            {t.originName ?? t.routeCode ?? '—'} → {t.destinationName ?? '—'}
          </td>
          <td>{t.materialName ?? '—'}</td>
          <td className="mono">{t.loadTonnes != null ? `${Math.round(t.loadTonnes)} t` : '—'}</td>
          <td className="mono">{formatDateTime(t.startedAt)}</td>
          <td className="mono">{formatDistance(t.distanceKm)}</td>
          <td>
            <span className={`badge ${STATUS_BADGE[t.status] ?? 'gray'}`}>
              {STATUS_LABELS[t.status] ?? t.status}
            </span>
          </td>
          <td style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
            {t.status === 'Active' && (
              <>
                <button className="btn" onClick={(e) => { e.stopPropagation(); onEdit(t) }} title="Editar material y carga">
                  <PencilIcon size={14} />
                </button>{' '}
                <button className="btn danger" onClick={(e) => { e.stopPropagation(); onCancel(t) }} title="Cancelar viaje">
                  <CloseIcon size={14} />
                </button>
              </>
            )}
          </td>
        </tr>
      ))}
    </DataTable>
  )
}