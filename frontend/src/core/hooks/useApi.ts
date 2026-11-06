import { useCallback, useEffect, useState } from 'react'
import type { ApiResult } from '../api/types'
import { summarizeMessages } from '../api/http/alerts'

/**
 * Carga datos de un endpoint del core/api. La función debe devolver el envelope
 * ApiResult<T> del backend; aquí se desenvuelve `data` y se exponen loading y
 * error, de modo que las páginas consuman el payload directo (`.data ?? []`).
 */
export function useApi<T>(fn: () => Promise<ApiResult<T>>, deps: unknown[] = []) {
  const [data, setData] = useState<T | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await fn()
      setData(result?.data ?? null)
      if (!result?.data) setError(summarizeMessages(result?.messages))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error inesperado')
    } finally {
      setLoading(false)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps)

  useEffect(() => {
    load()
  }, [load])

  return { data, loading, error, reload: load }
}