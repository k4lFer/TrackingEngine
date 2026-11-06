import { useState } from 'react'
import { materialsApi } from '../../core/api'
import { usePaged } from '../../core/hooks/usePaged'
import { useConfirm } from '../../core/hooks/useConfirm'
import { useToast } from '../../core/hooks/useToast'
import { summarizeMessages } from '../../core/api/http/alerts'
import type { Material } from '../../core/api/materials'
import PageHeader from '../../components/ui/PageHeader'
import Pagination from '../../components/ui/Pagination'
import { PlusIcon, SearchIcon } from '../../components/ui/Icons'
import MaterialTable from './components/MaterialTable'
import MaterialFormModal from './components/MaterialFormModal'

export default function Materials() {
  const [activeFilter, setActiveFilter] = useState('')
  const list = usePaged<Material>({
    fetch: (params) => materialsApi.list(params),
    pageSize: 20,
    filters: activeFilter === '' ? {} : { Active: activeFilter === 'true' },
  })
  const confirm = useConfirm()
  const { notify } = useToast()
  const [showCreate, setShowCreate] = useState(false)
  const [editing, setEditing] = useState<Material | null>(null)

  const removeItem = async (id: string) => {
    const ok = await confirm({
      title: 'Eliminar',
      message: '¿Eliminar este material?',
      confirmLabel: 'Eliminar',
      danger: true,
    })
    if (!ok) return
    try {
      const result = await materialsApi.remove(id)
      notify(summarizeMessages(result?.messages) ?? 'Material eliminado')
      list.reload()
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Error al eliminar', 'error')
    }
  }

  return (
    <div>
      <PageHeader
        title="Materiales"
        subtitle="Tipos de carga transportados por la flota"
        actions={
          <button className="btn primary" onClick={() => setShowCreate(true)}>
            <PlusIcon size={16} /> Nuevo material
          </button>
        }
      />

      <div className="table-toolbar">
        <h3 className="table-title">
          Materiales
          <span className="mono">({list.totalCount})</span>
        </h3>
        <div className="toolbar-controls">
          <select
            className="filter-select"
            value={activeFilter}
            onChange={(e) => {
              setActiveFilter(e.target.value)
              list.setPage(1)
            }}
          >
            <option value="">Activo e inactivo</option>
            <option value="true">Solo activos</option>
            <option value="false">Solo inactivos</option>
          </select>
          <div className="search-box">
            <SearchIcon size={14} />
            <input
              className="search-input"
              placeholder="Buscar por código o nombre…"
              value={list.search}
              onChange={(e) => list.setSearch(e.target.value)}
            />
          </div>
        </div>
      </div>

      <MaterialTable
        materials={list.items}
        loading={list.loading}
        error={list.error}
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

      {showCreate && (
        <MaterialFormModal
          onClose={() => setShowCreate(false)}
          onSaved={() => {
            setShowCreate(false)
            notify('Material creado')
            list.reload()
          }}
        />
      )}

      {editing && (
        <MaterialFormModal
          initial={editing}
          onClose={() => setEditing(null)}
          onSaved={() => {
            setEditing(null)
            notify('Material actualizado')
            list.reload()
          }}
        />
      )}
    </div>
  )
}