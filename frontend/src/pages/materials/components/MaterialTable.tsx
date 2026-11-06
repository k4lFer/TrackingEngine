import type { Material } from '../../../core/api/materials'
import DataTable from '../../../components/ui/DataTable'
import { PencilIcon, TrashIcon } from '../../../components/ui/Icons'

interface MaterialTableProps {
  materials: Material[]
  loading?: boolean
  error?: string | null
  onEdit: (material: Material) => void
  onRemove: (id: string) => void
}

export default function MaterialTable({ materials, loading, error, onEdit, onRemove }: MaterialTableProps) {
  return (
    <DataTable
      columns={['Código', 'Nombre', 'Unidad', 'Estado', '']}
      loading={loading}
      error={error}
      rows={materials.length}
      empty="No hay materiales registrados."
      emptyHint="Crea el primero para comenzar."
    >
      {materials.map((m) => (
        <tr key={m.id}>
          <td className="mono">{m.code}</td>
          <td>{m.name}</td>
          <td className="mono">{m.unit}</td>
          <td>
            <span className={`badge ${m.active ? 'green' : 'gray'}`}>
              {m.active ? 'Activo' : 'Inactivo'}
            </span>
          </td>
          <td style={{ textAlign: 'right' }}>
            <button className="btn" onClick={() => onEdit(m)} title="Editar">
              <PencilIcon size={14} />
            </button>{' '}
            <button className="btn danger" onClick={() => onRemove(m.id)}>
              <TrashIcon size={14} />
            </button>
          </td>
        </tr>
      ))}
    </DataTable>
  )
}