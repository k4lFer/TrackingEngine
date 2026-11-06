import { Link } from 'react-router-dom'
import { useDashboardData } from '../../core/hooks/useDashboardData'
import { useSignalR } from '../../core/hooks/useSignalR'
import FleetMap from '../../components/FleetMap'
import { FleetTable } from '../../components/ui/Fleet'
import PageHeader from '../../components/ui/PageHeader'
import { RefreshIcon } from '../../components/ui/Icons'

export default function Fleet() {
  const { dataVersion, vehicles: live, connected } = useSignalR()
  const { vehicles, geofences, routes, refresh } = useDashboardData(dataVersion, connected)
  const fleet = vehicles.data ?? []

  return (
    <div>
      <PageHeader
        title="Flota en línea"
        subtitle="Estado y última posición de todas las unidades"
        actions={
          <button className="btn" onClick={refresh}>
            <RefreshIcon size={14} />
            {connected ? ' En vivo' : ' Actualizar'}
          </button>
        }
      />

      {vehicles.error && <div className="error-box">{vehicles.error}</div>}

      <section className="card map-card" style={{ marginBottom: 16 }}>
        <div className="card-header">
          <h3>
            Posiciones inalámbricas
            <span className={`live-dot ${connected ? 'on' : ''}`} />
          </h3>
        </div>
        <div className="map-body">
          <FleetMap vehicles={fleet} geofences={geofences.data ?? []} routes={routes.data ?? []} live={live} />
        </div>
      </section>

      <div className="card">
        <div className="card-header">
          <h3>Todas las unidades ({fleet.length})</h3>
          <Link to="/vehicles" className="btn">Administrar</Link>
        </div>
        {vehicles.loading && <div className="loading">Cargando…</div>}
        {!vehicles.loading && fleet.length === 0 && (
          <div className="empty">
            Sin vehículos registrados. <Link to="/vehicles">Administrar vehículos</Link>
          </div>
        )}
        {fleet.length > 0 && <FleetTable vehicles={fleet} />}
      </div>
    </div>
  )
}
