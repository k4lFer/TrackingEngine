import { useCallback, useState } from 'react'

export function errorMessage(err: unknown, fallback: string): string {
  return err instanceof Error ? err.message : fallback
}

/**
 * Ejecuta una mutación del core/api (devuelve ApiResult) y expone un objeto
 * result con `ok` y `data` (payload desenvuelto). El error HTTP se captura
 * como `error` y se devuelve en `result`.
 */
export function useMutation<TArgs extends unknown[]>(fn: (...args: TArgs) => Promise<unknown>) {
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const run = useCallback(
    async (...args: TArgs) => {
      setSaving(true)
      setError(null)
      try {
        const result = await fn(...args)
        const envelope = result as { data?: unknown } | null | undefined
        const data = envelope && 'data' in envelope ? envelope.data : result
        return { ok: true as const, data }
      } catch (err) {
        setError(errorMessage(err, 'Ocurrió un error'))
        return { ok: false as const, error: errorMessage(err, 'Ocurrió un error') }
      } finally {
        setSaving(false)
      }
    },
    [fn],
  )

  const clearError = useCallback(() => setError(null), [])

  return { saving, error, run, clearError }
}