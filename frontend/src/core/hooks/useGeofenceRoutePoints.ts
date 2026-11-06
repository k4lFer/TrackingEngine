import { useMemo, useState } from 'react'
import type { CoordinateDto } from '../api/common'

interface GeofenceRoutePoints {
  points: CoordinateDto[]
  origin: CoordinateDto | null
  dest: CoordinateDto | null
  addPoint: (lat: number, lon: number) => void
  setOrigin: (pt: CoordinateDto | null) => void
  setDest: (pt: CoordinateDto | null) => void
  removeLast: () => void
  removePoint: (index: number) => void
  clear: () => void
  movePoint: (index: number, lat: number, lon: number) => void
}

export function useGeofenceRoutePoints(initial: CoordinateDto[] = []): GeofenceRoutePoints {
  const [origin, setOrigin] = useState<CoordinateDto | null>(initial[0] ?? null)
  const [middle, setMiddle] = useState<CoordinateDto[]>(initial.length > 2 ? initial.slice(1, -1) : [])
  const [dest, setDest] = useState<CoordinateDto | null>(initial.length >= 2 ? initial[initial.length - 1] : null)

  const points = useMemo<CoordinateDto[]>(
    () => [...(origin ? [origin] : []), ...middle, ...(dest ? [dest] : [])],
    [origin, middle, dest],
  )

  const addPoint = (lat: number, lon: number) => setMiddle((m) => [...m, { lat, lon }])

  const removeLast = () => {
    if (middle.length > 0) setMiddle((m) => m.slice(0, -1))
    else if (dest) setDest(null)
    else if (origin) setOrigin(null)
  }

  const removePoint = (index: number) => {
    const originOffset = origin ? 1 : 0
    const destStart = points.length - (dest ? 1 : 0)
    if (index < originOffset) {
      setOrigin(null)
      return
    }
    if (index >= destStart) {
      setDest(null)
      return
    }
    const middleIndex = index - originOffset
    setMiddle((m) => m.filter((_, i) => i !== middleIndex))
  }

  const clear = () => {
    setOrigin(null)
    setMiddle([])
    setDest(null)
  }

  const movePoint = (index: number, lat: number, lon: number) => {
    const middleStart = origin ? 1 : 0
    if (index < middleStart) return
    if (dest && index >= points.length - 1) return
    const middleIndex = index - middleStart
    setMiddle((m) => m.map((p, i) => (i === middleIndex ? { lat, lon } : p)))
  }

  return { points, origin, dest, addPoint, setOrigin, setDest, removeLast, removePoint, clear, movePoint }
}