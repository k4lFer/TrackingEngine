import type { TrackingEvent } from '../../core/api/events'
import { EVENT_LABELS, timeAgo } from '../../core/format'
import { TH, TD, TABLE } from './table'

export function severityBadge(severity: string): string {
  if (severity === 'Critical') return 'red'
  if (severity === 'Warning') return 'amber'
  return 'blue'
}

export function EventTable({ events }: { events: TrackingEvent[] }) {
  return (
    <table className={TABLE}>
      <thead>
        <tr>
          <th className={TH}>Hora</th>
          <th className={TH}>Unidad</th>
          <th className={TH}>Evento</th>
          <th className={TH}>Severidad</th>
        </tr>
      </thead>
      <tbody>
        {events.map((e) => (
          <tr key={e.id}>
            <td className={`${TD} mono`}>{timeAgo(e.occurredAt)}</td>
            <td className={`${TD} mono`}>{e.vehicleCode}</td>
            <td className={TD}>{EVENT_LABELS[e.type] ?? e.type}</td>
            <td className={TD}>
              <span className={`badge ${severityBadge(e.severity)}`}>{e.severity}</span>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}