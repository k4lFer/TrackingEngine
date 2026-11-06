import { Link } from 'react-router-dom'
import { useDashboardData } from '../../core/hooks/useDashboardData'
import { useSignalR } from '../../core/hooks/useSignalR'
import FleetMap from '../../components/FleetMap'
import Stats from '../../components/ui/Stats'
import { EventTable } from '../../components/ui/Events'
import { FleetTable } from '../../components/ui/Fleet'
import { RefreshIcon } from '../../components/ui/Icons'

export default function Dashboard() {
  const { vehicles, geofences, routes, events, refresh } = useDashboardData()
  const { vehicles: live, connected, dataVersion } = useSignalR()

  const fleet = vehicles.data ?? []
  const liveList = Object.values(live)
  const moving = liveList.length > 0 ? liveList.filter((v) => v.speedKmh != null && v.speedKmh >= 3).length
    : fleet.filter((v) => v.state === 'EnRuta').length
  const tonnes = Math.round(liveList.reduce((acc, v) => acc + (v.loadTonnes ?? 0), 0))
  const critical = (events.data?.events ?? []).filter((e) => e.severity === 'Critical').length

  void dataVersion

  return (
    <div className="dashboard">
      {vehicles.error && <div className="error-box">{vehicles.error}</div>}

      <Stats
        items={[
          { label: 'Unidades', value: fleet.length },
          { label: 'En movimiento', value: moving, tone: 'ok' },
          { label: 'Toneladas en ruta', value: tonnes ? `${tonnes} t` : '0 t', tone: 'ok' },
          { label: 'Alertas crÃ­ticas', value: critical, tone: 'alert' },
        ]}
      />

      <section className="card map-card mb-4">
        <div className="card-header">
          <h3 className="flex items-center gap-2">
            <span>PosiciÃ³n en vivo</span>
            <span className={`live-dot ${connected ? 'on' : ''}`} />
          </h3>
          <button className="btn" onClick={refresh}>
            <RefreshIcon size={14} /> Actualizar
          </button>
        </div>
        <div className="map-body">
          <FleetMap vehicles={fleet} geofences={geofences.data ?? []} routes={routes.data ?? []} live={live} />
        </div>
      </section>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
        <section className="card fleet-card">
          <div className="card-header">
            <h3>Flota</h3>
            <Link to="/fleet" className="btn">Ver todo</Link>
          </div>
          {vehicles.loading && <div className="py-10 text-center text-ink-dim text-sm">Cargandoâ€¦</div>}
          {!vehicles.loading && fleet.length === 0 && <div className="py-10 text-center text-ink-dim text-sm">Sin vehÃ­culos</div>}
          {fleet.length > 0 && <FleetTable vehicles={fleet} />}
        </section>

        <section className="card fleet-card">
          <div className="card-header">
            <h3>Eventos recientes</h3>
            <Link to="/events" className="btn">Ver todo</Link>
          </div>
          {events.loading && <div className="py-10 text-center text-ink-dim text-sm">Cargandoâ€¦</div>}
          {!events.loading && (events.data?.events ?? []).length === 0 && (
            <div className="py-10 text-center text-ink-dim text-sm">Sin eventos</div>
          )}
          {!events.loading && (events.data?.events ?? []).length > 0 && (
            <EventTable events={(events.data?.events ?? []).slice(0, 5)} />
          )}
        </section>
      </div>
    </div>
  )
}
