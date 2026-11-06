import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { TripSummary, UpdateTripInput } from './types';

/** PUT /trips/{id} — edita material/carga de un viaje en curso. */
export function update(id: string, input: UpdateTripInput): Promise<ApiResult<TripSummary>> {
  return apiRequest<ApiResult<TripSummary>>(`/trips/${id}`, { method: 'PUT', body: input });
}