import { useApi } from '../../core/hooks/useApi'
import { usePolling } from '../../core/hooks/usePolling'
import { useSignalR } from '../../core/hooks/useSignalR'
import { eventsApi } from '../../core/api'
import { EventTable } from '../../components/ui/Events'
import PageHeader from '../../components/ui/PageHeader'
import { RefreshIcon } from '../../components/ui/Icons'

export default function Events() {
  const { tick, refresh } = usePolling(5000)
  const { dataVersion } = useSignalR()
  const { data, loading, error } = useApi(() => eventsApi.recent(100), [tick, dataVersion])
  const events = data?.events ?? []

  return (
    <div>
      <PageHeader
        title="Eventos y alarmas"
        subtitle="Registro de eventos operacionales de las unidades"
        actions={
          <button className="btn" onClick={refresh}>
            <RefreshIcon size={14} /> Actualizar
          </button>
        }
      />

      <div className="card">
        <div className="table-wrap">
          {error && <div className="error-box">{error}</div>}
          {loading && <div className="loading">Cargando…</div>}
          {!loading && !error && events.length === 0 && (
            <div className="empty">Sin eventos por el momento.</div>
          )}
          {!loading && !error && events.length > 0 && (
            <EventTable events={events} />
          )}
        </div>
      </div>
    </div>
  )
}
