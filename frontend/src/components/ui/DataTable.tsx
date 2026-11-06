import type { ReactNode } from 'react'
import { TH, TABLE, TABLE_WRAP } from './table'

interface DataTableProps {
  columns: string[]
  loading?: boolean
  error?: string | null
  empty?: string
  emptyHint?: string
  rows?: number
  children: ReactNode
}

export default function DataTable({
  columns,
  loading,
  error,
  empty = 'Sin registros.',
  emptyHint,
  rows = 0,
  children,
}: DataTableProps) {
  return (
    <div className={TABLE_WRAP}>
      {error && <div className="error-box">{error}</div>}
      {loading && <div className="py-10 text-center text-ink-dim text-sm">Cargando...</div>}
      {!loading && !error && rows === 0 && (
        <div className="py-10 text-center">
          <div className="text-ink-dim">{empty}</div>
          {emptyHint && <div className="mt-1 text-xs text-ink-faint">{emptyHint}</div>}
        </div>
      )}
      {!loading && !error && rows > 0 && (
        <table className={TABLE}>
          <thead>
            <tr>
              {columns.map((c) => (
                <th key={c} className={TH}>
                  {c}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>{children}</tbody>
        </table>
      )}
    </div>
  )
}