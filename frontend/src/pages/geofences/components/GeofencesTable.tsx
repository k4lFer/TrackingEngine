import type { Geofence } from '../../../core/api/geofences'
import DataTable from '../../../components/ui/DataTable'
import { PencilIcon, TrashIcon } from '../../../components/ui/Icons'
import { FILL_COLOR } from '../geofenceConstants'

interface GeofencesTableProps {
  geofences: Geofence[]
  loading?: boolean
  error?: string | null
  onEdit: (geofence: Geofence) => void
  onRemove: (id: string) => void
}

export default function GeofencesTable({ geofences, loading, error, onEdit, onRemove }: GeofencesTableProps) {
  return (
    <DataTable
      columns={['Nombre', 'Código', 'Tipo', 'Prioridad', 'Vel. máx', '']}
      loading={loading}
      error={error}
      rows={geofences.length}
      empty="No hay geocercas."
      emptyHint="Dibuja una para empezar."
    >
      {geofences.map((g) => (
        <tr key={g.id}>
          <td>
            <span
              style={{
                display: 'inline-block',
                width: 10,
                height: 10,
                borderRadius: 3,
                background: g.color || FILL_COLOR,
                marginRight: 8,
              }}
            />
            {g.name}
          </td>
          <td className="mono">{g.code}</td>
          <td>
            <span className="badge blue">{g.kind}</span>
          </td>
          <td className="mono">{g.priority}</td>
          <td className="mono">{g.maxSpeedKmh != null ? `${g.maxSpeedKmh} km/h` : '—'}</td>
          <td style={{ textAlign: 'right' }}>
            <button className="btn" onClick={() => onEdit(g)} title="Editar">
              <PencilIcon size={14} />
            </button>{' '}
            <button className="btn danger" onClick={() => onRemove(g.id)}>
              <TrashIcon size={14} />
            </button>
          </td>
        </tr>
      ))}
    </DataTable>
  )
}