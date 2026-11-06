import type { VehicleStatus } from '../../core/api/vehicles'
import { STATE_LABELS, formatDuration, timeAgo } from '../../core/format'
import { TH, TD, TABLE } from './table'

export function stateBadge(state: string): string {
  if (state === 'Moving' || state === 'EnRuta' || state === 'EnCarga' || state === 'EnDescarga') return 'green'
  if (state === 'Idle' || state === 'Stopped' || state === 'Detenido') return 'amber'
  return 'gray'
}

function stoppedSeconds(stoppedSince: string): number {
  return Math.max(0, Math.floor((Date.now() - new Date(stoppedSince).getTime()) / 1000))
}

export function FleetTable({ vehicles, onRowClick }: { vehicles: VehicleStatus[]; onRowClick?: (id: string) => void }) {
  return (
    <table className={TABLE}>
      <thead>
        <tr>
          <th className={TH}>Unidad</th>
          <th className={TH}>Placa</th>
          <th className={TH}>Estado</th>
          <th className={TH}>Última señal</th>
        </tr>
      </thead>
      <tbody>
        {vehicles.map((v) => (
          <tr
            key={v.id}
            onClick={onRowClick ? () => onRowClick(v.id) : undefined}
            className={onRowClick ? 'cursor-pointer' : undefined}
          >
            <td className={`${TD} mono`}>{v.code}</td>
            <td className={`${TD} mono text-ink-dim`}>{v.plate}</td>
            <td className={TD}>
              <span className={`badge ${stateBadge(v.state)}`}>
                {STATE_LABELS[v.state] ?? v.state}
              </span>
              {v.state === 'Detenido' && v.stoppedSince && (
                <div className="text-ink-dim text-xs mt-1">
                  {formatDuration(stoppedSeconds(v.stoppedSince))}
                </div>
              )}
            </td>
            <td className={`${TD} mono text-ink-dim`}>
              {timeAgo(v.lastReportedAt)}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}