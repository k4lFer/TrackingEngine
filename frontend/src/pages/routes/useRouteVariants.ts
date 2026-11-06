import { useEffect, useState } from 'react'
import { routesApi } from '../../core/api'
import type { RouteAlternativeItem, RouteItem } from '../../core/api/routes'

export function useRouteVariants(items: RouteItem[], reload: () => void) {
  const [variants, setVariants] = useState<RouteAlternativeItem[]>([])

  useEffect(() => {
    const principals = (items ?? []).filter((r) => (r.alternatives?.length ?? 0) > 0)
    if (principals.length === 0) {
      setVariants([])
      return
    }
    let disposed = false
    Promise.all(
      principals.map(async (r) => {
        try {
          const result = await routesApi.getById(r.id)
          return (result.data?.alternatives ?? []).filter((a) => a.geoJsonLine)
        } catch {
          return []
        }
      }),
    ).then((groups) => {
      if (!disposed) setVariants(groups.flat())
    })
    return () => {
      disposed = true
    }
  }, [items, reload])

  return variants
}