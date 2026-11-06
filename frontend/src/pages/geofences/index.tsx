import { useState } from 'react'
import { geofencesApi } from '../../core/api'
import { useCrud } from '../../core/hooks/useCrud'
import type { Geofence } from '../../core/api/geofences'
import PageHeader from '../../components/ui/PageHeader'
import { PlusIcon } from '../../components/ui/Icons'
import GeofencesMap from './components/GeofencesMap'
import GeofencesTable from './components/GeofencesTable'
import GeofenceFormModal from './components/GeofenceFormModal'

export default function Geofences() {
  const crud = useCrud<Geofence>({
    list: () => geofencesApi.list(),
    remove: (id) => geofencesApi.remove(id),
    deleteConfirm: '¿Eliminar esta geocerca?',
    deletedMessage: 'Geocerca eliminada',
  })
  const [showCreate, setShowCreate] = useState(false)
  const [editing, setEditing] = useState<Geofence | null>(null)

  return (
    <div>
      <PageHeader
        title="Geocercas"
        subtitle="Zonas de carga, descarga y restricciones en el terreno"
        actions={
          <button className="btn primary" onClick={() => setShowCreate(true)}>
            <PlusIcon size={16} /> Nueva geocerca
          </button>
        }
      />

      <section className="card map-card" style={{ marginBottom: 16 }}>
        <div className="card-header">
          <h3>Zonas en el terreno</h3>
          <span className="mono">{crud.data?.length ?? 0} geocercas</span>
        </div>
        <div className="map-body">
          <GeofencesMap items={crud.data ?? []} />
        </div>
      </section>

      <GeofencesTable
        geofences={crud.data ?? []}
        loading={crud.loading}
        error={crud.error}
        onEdit={setEditing}
        onRemove={crud.removeItem}
      />

      {showCreate && (
        <GeofenceFormModal
          existing={crud.data ?? []}
          onClose={() => setShowCreate(false)}
          onSaved={() => {
            setShowCreate(false)
            crud.notify('Geocerca creada')
            crud.reload()
          }}
        />
      )}

      {editing && (
        <GeofenceFormModal
          initial={editing}
          existing={crud.data ?? []}
          onClose={() => setEditing(null)}
          onSaved={() => {
            setEditing(null)
            crud.notify('Geocerca actualizada')
            crud.reload()
          }}
        />
      )}
    </div>
  )
}