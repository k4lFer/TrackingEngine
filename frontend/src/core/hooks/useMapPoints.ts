import { useCallback, useState } from 'react'
import type { CoordinateDto } from '../api/common'

export function useMapPoints(initial: CoordinateDto[] = []) {
  const [points, setPoints] = useState<CoordinateDto[]>(initial)

  const addPoint = useCallback((lat: number, lon: number) => {
    setPoints((p) => [...p, { lat, lon }])
  }, [])

  const removeLast = useCallback(() => {
    setPoints((p) => p.slice(0, -1))
  }, [])

  const clear = useCallback(() => setPoints([]), [])

  const set = useCallback((next: CoordinateDto[]) => setPoints(next), [])

  const movePoint = useCallback((index: number, lat: number, lon: number) => {
    setPoints((p) => p.map((pt, i) => (i === index ? { lat, lon } : pt)))
  }, [])

  const removePoint = useCallback((index: number) => {
    setPoints((p) => p.filter((_, i) => i !== index))
  }, [])

  return { points, addPoint, removeLast, clear, set, movePoint, removePoint }
}