import type { Vehicle } from '../../../core/api/vehicles'
import DataTable from '../../../components/ui/DataTable'
import { stateBadge } from '../../../components/ui/Fleet'
import { STATE_LABELS } from '../../../core/format'
import { EyeIcon, PencilIcon, TrashIcon } from '../../../components/ui/Icons'

interface VehicleTableProps {
  vehicles: Vehicle[]
  loading?: boolean
  error?: string | null
  onView: (vehicle: Vehicle) => void
  onEdit: (vehicle: Vehicle) => void
  onRemove: (id: string) => void
}

export default function VehicleTable({ vehicles, loading, error, onView, onEdit, onRemove }: VehicleTableProps) {
  return (
    <DataTable
      columns={['Código', 'Placa', 'Marca / Modelo', 'Dispositivo', 'Estado', '']}
      loading={loading}
      error={error}
      rows={vehicles.length}
      empty="No hay vehículos registrados."
      emptyHint="Crea el primero para comenzar."
    >
      {vehicles.map((v) => (
        <tr key={v.id}>
          <td className="mono">{v.code}</td>
          <td className="mono">{v.plate}</td>
          <td>
            {v.brand} {v.model}
          </td>
          <td className="mono">
            {v.deviceIdentifier != null ? (
              <span title={`Dispositivo ${v.deviceIdentifier}${v.deviceModel ? ` · ${v.deviceModel}` : ''}`}>
                {v.deviceIdentifier}
              </span>
            ) : (
              <span className="text-[11px] text-ink-faint">—</span>
            )}
          </td>
          <td>
            <span className={`badge ${stateBadge(v.state)}`}>{STATE_LABELS[v.state] ?? v.state}</span>
            {!v.active && <span className="text-[11px] text-ink-faint ml-1.5">· inactivo</span>}
          </td>
          <td style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
            <button className="btn" onClick={() => onView(v)} title="Ver detalle e historial">
              <EyeIcon size={14} />
            </button>{' '}
            <button className="btn" onClick={() => onEdit(v)} title="Editar">
              <PencilIcon size={14} />
            </button>{' '}
            <button className="btn danger" onClick={() => onRemove(v.id)}>
              <TrashIcon size={14} />
            </button>
          </td>
        </tr>
      ))}
    </DataTable>
  )
}