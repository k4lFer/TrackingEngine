import { useCallback, useEffect, useRef, useState } from 'react'
import type { PagedList } from '../api/http/client'

export interface PagedParams {
  NumberPage: number
  PageSize: number
  Search: string
  State?: string
  Active?: boolean
  Status?: string
  FromDate?: string
  ToDate?: string
}

export type PagedFilters = Partial<Pick<PagedParams, 'State' | 'Active' | 'Status' | 'FromDate' | 'ToDate'>>

interface UsePagedOptions<T> {
  fetch: (params: PagedParams) => Promise<PagedList<T>>
  /** Tamaño de página por defecto para las tablas admin. */
  pageSize?: number
  searchDebounceMs?: number
  /** Filtros opcionales (p. ej. State o Active); al cambiar se vuelve a buscar. */
  filters?: PagedFilters
}

/**
 * Listado paginado con buscador (debounced). Mantiene página, totales y estado
 * de carga, y vuelve a la página anterior si la actual queda vacía tras un
 * borrado. Página y búsqueda se pueden modificar desde el componente.
 */
export function usePaged<T>({ fetch, pageSize = 20, searchDebounceMs = 300, filters }: UsePagedOptions<T>) {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [version, setVersion] = useState(0)

  const [items, setItems] = useState<T[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchRef = useRef(fetch)
  useEffect(() => {
    fetchRef.current = fetch
  })

  useEffect(() => {
    if (search === debouncedSearch) return
    const id = setTimeout(() => {
      setDebouncedSearch(search)
      setPage(1)
    }, searchDebounceMs)
    return () => clearTimeout(id)
  }, [search, debouncedSearch, searchDebounceMs])

  const load = useCallback(() => {
    let active = true
    setLoading(true)
    fetchRef
      .current({
        NumberPage: page,
        PageSize: pageSize,
        Search: debouncedSearch,
        ...(filters?.State != null ? { State: filters.State } : {}),
        ...(filters?.Active != null ? { Active: filters.Active } : {}),
        ...(filters?.Status != null ? { Status: filters.Status } : {}),
        ...(filters?.FromDate != null ? { FromDate: filters.FromDate } : {}),
        ...(filters?.ToDate != null ? { ToDate: filters.ToDate } : {}),
      })
      .then((res) => {
        if (!active) return
        setItems(res.data ?? [])
        setTotalCount(res.totalCount)
        setTotalPages(Math.max(res.totalPages, 1))
        setError(null)
        if (page > 1 && (res.data?.length ?? 0) === 0 && res.totalCount > 0) {
          setPage((p) => Math.max(p - 1, 1))
        }
      })
      .catch((err) => {
        if (!active) return
        setError(err instanceof Error ? err.message : 'Error al cargar')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => {
      active = false
    }
  }, [page, pageSize, debouncedSearch, filters?.State, filters?.Active, filters?.Status, filters?.FromDate, filters?.ToDate])

  useEffect(() => {
    const cancel = load()
    return cancel
  }, [load, version])

  return {
    items,
    totalCount,
    totalPages,
    page,
    pageSize,
    search,
    setSearch,
    setPage,
    loading,
    error,
    reload: () => setVersion((v) => v + 1),
  }
}