import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { TripDetail } from './types';

/** GET /trips/{id} — detalle de un viaje (resumen + track + alertas). */
export function getById(id: string): Promise<ApiResult<TripDetail>> {
  return apiRequest<ApiResult<TripDetail>>(`/trips/${id}`);
}