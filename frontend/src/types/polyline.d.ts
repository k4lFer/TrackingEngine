declare module '@mapbox/polyline' {
  export interface Point {
    lat: number
    lon: number
  }

  export function decode(str: string, precision?: number): [number, number][]
  export function encode(coordinates: [number, number][], precision?: number): string
  export function fromGeoJSON(geojson: object, precision?: number): string
  export function toGeoJSON(str: string, precision?: number): object
}
