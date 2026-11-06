import { apiListPaged, type PagedList } from '../http/client';
import type { VehicleStatus } from './types';

/** Query de GET /vehicles/status (mismo VehicleFilterDto del backend). */
export interface StatusQuery {
  NumberPage?: number;
  PageSize?: number;
  Search?: string;
  State?: string;
  /** true = solo vehículos en línea (excluye Offline y FueraDeServicio). */
  OnlyOnline?: boolean;
}

/** GET /vehicles/status — flota con última posición (listado paginado). */
export function status(query: StatusQuery = {}): Promise<PagedList<VehicleStatus>> {
  return apiListPaged<VehicleStatus>('/vehicles/status', { query: { ...query } });
}