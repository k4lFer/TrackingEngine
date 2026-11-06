import { useCallback, useEffect, useState } from 'react'

export function usePolling(intervalMs: number, enabled = true) {
  const [tick, setTick] = useState(0)

  const refresh = useCallback(() => setTick((t) => t + 1), [])

  useEffect(() => {
    if (!enabled) return
    const id = setInterval(refresh, intervalMs)
    return () => clearInterval(id)
  }, [intervalMs, enabled, refresh])

  return { tick, refresh }
}
