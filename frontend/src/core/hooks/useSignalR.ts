import * as signalR from '@microsoft/signalr'
import { useCallback, useEffect, useSyncExternalStore } from 'react'
import { TRACKING_HUB_URL } from '../api/http/client'
import type { TripLive, VehicleLive } from '../api/live'

interface LiveStore {
  vehicles: Record<string, VehicleLive>
  lastTripChange: TripLive | null
  connected: boolean
  dataVersion: number
}

// Store compartido a nivel de módulo: cada página que usa useSignalR se suscribe y
// recibe TODOS los eventos. Un slot único por montaje rompía el render: al navegar,
// el timeout/deadle de los setters terminaba apuntando a una página desmontada y el
// socket seguía recibiendo sin que nada actualizara la UI.
let store: LiveStore = {
  vehicles: {},
  lastTripChange: null,
  connected: false,
  dataVersion: 0,
}

const listeners = new Set<() => void>()

function setStore(patch: Partial<LiveStore>) {
  store = { ...store, ...patch }
  listeners.forEach((l) => l())
}

// Los handlers se registran al CREAR la conexión, nunca desde el interior de un
// useEffect. Así la conexión nunca puede recibir eventos sin handlers colgados
// ("No client method with the name 'vehiclemoved' found.").
let shared: signalR.HubConnection | null = null
let sharedStart: Promise<void> | null = null

function createConnection(): signalR.HubConnection {
  const conn = new signalR.HubConnectionBuilder()
    .withUrl(TRACKING_HUB_URL)
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  conn.on('vehicleMoved', (dto: VehicleLive) => {
    setStore({ vehicles: { ...store.vehicles, [dto.vehicleId]: dto } })
  })
  conn.on('tripChanged', (dto: TripLive) => {
    setStore({ lastTripChange: dto, dataVersion: store.dataVersion + 1 })
  })
  conn.on('dataChanged', () => setStore({ dataVersion: store.dataVersion + 1 }))
  conn.on('Connected', () => setStore({ connected: true }))
  conn.onreconnecting(() => setStore({ connected: false }))
  conn.onreconnected(() => setStore({ connected: true }))
  conn.onclose(() => setStore({ connected: false }))

  return conn
}

function ensureShared(): { ready: Promise<void> } {
  if (shared && sharedStart) return { ready: sharedStart }

  const conn = createConnection()
  shared = conn
  sharedStart = conn
    .start()
    .then(() => setStore({ connected: true }))
    .catch((err) => {
      shared = null
      sharedStart = null
      setStore({ connected: false })
      throw err
    })

  return { ready: sharedStart }
}

export function useSignalR() {
  useEffect(() => {
    ensureShared().ready.catch(() => setStore({ connected: false }))
  }, [])

  const subscribe = useCallback((cb: () => void) => {
    listeners.add(cb)
    return () => {
      listeners.delete(cb)
    }
  }, [])

  const { vehicles, lastTripChange, connected, dataVersion } = useSyncExternalStore(
    subscribe,
    () => store,
    () => store,
  )

  const bumpData = useCallback(() => setStore({ dataVersion: store.dataVersion + 1 }), [])

  return { vehicles, lastTripChange, connected, dataVersion, bumpData }
}