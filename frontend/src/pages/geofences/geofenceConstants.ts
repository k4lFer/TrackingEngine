export const KINDS = ['Load', 'Unload', 'Wait', 'Restricted', 'Checkpoint', 'Custom']

export const KIND_LABELS: Record<string, string> = {
  Load: 'Carga',
  Unload: 'Descarga',
  Wait: 'Espera',
  Restricted: 'Restringida',
  Checkpoint: 'Control',
  Custom: 'Personalizada',
}

export const KIND_HINTS: Record<string, string> = {
  Load: 'Un vehiculo quieto aqui se marca En carga, no Detenido.',
  Unload: 'Un vehiculo quieto aqui se marca En descarga, no Detenido.',
  Wait: 'Un vehiculo quieto aqui queda Disponible, no Detenido.',
  Restricted: 'Un vehiculo quieto aqui cuenta como Detenido.',
  Checkpoint: 'Un vehiculo quieto aqui cuenta como Detenido.',
  Custom: 'Un vehiculo quieto aqui cuenta como Detenido.',
}

export const FILL_COLOR = '#e09b3d'
export const GEO_CENTER: [number, number] = [-13.53, -72.87]

export const SHAPES = ['polygon', 'rectangle', 'circle'] as const
export type GeofenceShape = (typeof SHAPES)[number]
