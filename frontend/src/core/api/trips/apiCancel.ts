import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { TripSummary } from './types';

/** POST /trips/{id}/cancel — cancela un viaje en curso. */
export function cancel(id: string): Promise<ApiResult<TripSummary>> {
  return apiRequest<ApiResult<TripSummary>>(`/trips/${id}/cancel`, { method: 'POST' });
}