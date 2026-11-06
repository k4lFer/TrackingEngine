import { useState } from 'react'
import { geofencesApi, routesApi } from '../../core/api'
import { useCrud } from '../../core/hooks/useCrud'
import { useApi } from '../../core/hooks/useApi'
import type { Geofence } from '../../core/api/geofences'
import type { RouteItem } from '../../core/api/routes'
import PageHeader from '../../components/ui/PageHeader'
import { PlusIcon } from '../../components/ui/Icons'
import RoutesMap from './components/RoutesMap'
import RouteTable from './components/RouteTable'
import RouteFormModal from './components/RouteFormModal'
import { useRouteVariants } from './useRouteVariants'

export default function Routes() {
  const crud = useCrud<RouteItem>({
    list: () => routesApi.list(),
    remove: (id) => routesApi.remove(id),
    deleteConfirm: '¿Eliminar esta ruta?',
    deletedMessage: 'Ruta eliminada',
  })
  const geofences = useApi<Geofence[]>(() => geofencesApi.list(), [])
  const [showCreate, setShowCreate] = useState(false)
  const [editing, setEditing] = useState<RouteItem | null>(null)
  const variants = useRouteVariants(crud.data ?? [], crud.reload)

  return (
    <div>
      <PageHeader
        title="Rutas"
        subtitle="Trayectos definidos entre puntos de operación"
        actions={
          <button className="btn primary" onClick={() => setShowCreate(true)}>
            <PlusIcon size={16} /> Nueva ruta
          </button>
        }
      />

      <section className="card map-card" style={{ marginBottom: 16 }}>
        <div className="card-header">
          <h3>Rutas</h3>
          <span className="mono">{crud.data?.length ?? 0} rutas</span>
        </div>
        <div className="map-body">
          <RoutesMap
            items={crud.data ?? []}
            variants={variants}
            geofences={geofences.data ?? []}
          />
        </div>
      </section>

      <RouteTable
        routes={crud.data ?? []}
        loading={crud.loading}
        error={crud.error}
        onEdit={setEditing}
        onRemove={crud.removeItem}
      />

      {showCreate && (
        <RouteFormModal
          geofences={geofences.data ?? []}
          onClose={() => setShowCreate(false)}
          onSaved={() => {
            setShowCreate(false)
            crud.notify('Ruta creada')
            crud.reload()
            geofences.reload()
          }}
        />
      )}

      {editing && (
        <RouteFormModal
          initial={editing}
          geofences={geofences.data ?? []}
          onClose={() => setEditing(null)}
          onSaved={() => {
            setEditing(null)
            crud.notify('Ruta actualizada')
            crud.reload()
            geofences.reload()
          }}
        />
      )}
    </div>
  )
}