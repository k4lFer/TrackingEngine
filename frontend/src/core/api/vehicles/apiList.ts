import { apiListPaged, type PagedList } from '../http/client';
import type { Vehicle } from './types';

/** Query de GET /vehicles (refleja VehicleFilterDto del backend). */
export interface VehicleQuery {
  NumberPage?: number;
  PageSize?: number;
  Search?: string;
  State?: string;
}

/** GET /vehicles — listado paginado + metadatos para el paginador. */
export function list(query: VehicleQuery = {}): Promise<PagedList<Vehicle>> {
  return apiListPaged<Vehicle>('/vehicles', { query: { ...query } });
}