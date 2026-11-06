import { useState } from 'react'
import { vehiclesApi } from '../../core/api'
import { usePaged } from '../../core/hooks/usePaged'
import { useConfirm } from '../../core/hooks/useConfirm'
import { useToast } from '../../core/hooks/useToast'
import { summarizeMessages } from '../../core/api/http/alerts'
import type { Vehicle } from '../../core/api/vehicles'
import PageHeader from '../../components/ui/PageHeader'
import Pagination from '../../components/ui/Pagination'
import { PlusIcon, SearchIcon } from '../../components/ui/Icons'
import VehicleTable from './components/VehicleTable'
import VehicleFormModal from './components/VehicleFormModal'
import VehicleDetailModal from './components/VehicleDetailModal'

const STATE_FILTERS: { value: string; label: string }[] = [
  { value: '', label: 'Todos los estados' },
  { value: 'Disponible', label: 'Disponible' },
  { value: 'EnCarga', label: 'En carga' },
  { value: 'EnRuta', label: 'En ruta' },
  { value: 'Detenido', label: 'Detenido' },
  { value: 'EnDescarga', label: 'En descarga' },
  { value: 'FueraDeServicio', label: 'Fuera de servicio' },
  { value: 'Offline', label: 'Sin señal' },
]

export default function Vehicles() {
  const [stateFilter, setStateFilter] = useState('')
  const list = usePaged<Vehicle>({
    fetch: (params) => vehiclesApi.list(params),
    pageSize: 20,
    filters: stateFilter ? { State: stateFilter } : {},
  })
  const confirm = useConfirm()
  const { notify } = useToast()
  const [showCreate, setShowCreate] = useState(false)
  const [editing, setEditing] = useState<Vehicle | null>(null)
  const [viewing, setViewing] = useState<Vehicle | null>(null)

  const removeItem = async (id: string) => {
    const ok = await confirm({
      title: 'Eliminar',
      message: '¿Eliminar este vehículo?',
      confirmLabel: 'Eliminar',
      danger: true,
    })
    if (!ok) return
    try {
      const result = await vehiclesApi.remove(id)
      notify(summarizeMessages(result?.messages) ?? 'Vehículo eliminado')
      list.reload()
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Error al eliminar', 'error')
    }
  }

  return (
    <div>
      <PageHeader
        title="Vehículos de la flota"
        subtitle="Unidades de transporte de mineral registradas"
        actions={
          <button className="btn primary" onClick={() => setShowCreate(true)}>
            <PlusIcon size={16} /> Nuevo vehículo
          </button>
        }
      />

      <div className="table-toolbar">
        <h3 className="table-title">
          Vehículos
          <span className="mono">({list.totalCount})</span>
        </h3>
        <div className="toolbar-controls">
          <select
            className="filter-select"
            value={stateFilter}
            onChange={(e) => {
              setStateFilter(e.target.value)
              list.setPage(1)
            }}
          >
            {STATE_FILTERS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
          <div className="search-box">
            <SearchIcon size={14} />
            <input
              className="search-input"
              placeholder="Buscar por código o placa…"
              value={list.search}
              onChange={(e) => list.setSearch(e.target.value)}
            />
          </div>
        </div>
      </div>

      <VehicleTable
        vehicles={list.items}
        loading={list.loading}
        error={list.error}
        onView={setViewing}
        onEdit={setEditing}
        onRemove={removeItem}
      />

      <Pagination
        page={list.page}
        totalPages={list.totalPages}
        totalCount={list.totalCount}
        pageSize={list.pageSize}
        onPageChange={list.setPage}
      />

      {viewing && <VehicleDetailModal vehicleId={viewing.id} onClose={() => setViewing(null)} />}

      {showCreate && (
        <VehicleFormModal
          onClose={() => setShowCreate(false)}
          onSaved={() => {
            setShowCreate(false)
            notify('Vehículo creado')
            list.reload()
          }}
        />
      )}

      {editing && (
        <VehicleFormModal
          initial={editing}
          onClose={() => setEditing(null)}
          onSaved={() => {
            setEditing(null)
            notify('Vehículo actualizado')
            list.reload()
          }}
        />
      )}
    </div>
  )
}