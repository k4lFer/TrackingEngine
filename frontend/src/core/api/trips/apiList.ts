import { apiListPaged, type PagedList } from '../http/client';
import type { TripSummary } from './types';

/** Query de GET /trips (refleja TripFilterDto del backend). */
export interface TripListQuery {
  NumberPage?: number;
  PageSize?: number;
  Search?: string;
  Status?: string;
  FromDate?: string;
  ToDate?: string;
}

/** GET /trips — historial de viajes paginado + metadatos para el paginador. */
export function list(query: TripListQuery = {}): Promise<PagedList<TripSummary>> {
  return apiListPaged<TripSummary>('/trips', { query: { ...query } });
}