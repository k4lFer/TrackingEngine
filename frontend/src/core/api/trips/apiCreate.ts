import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { CreateTripInput, TripSummary } from './types';

/** POST /trips — inicia un viaje de forma manual. */
export function create(input: CreateTripInput): Promise<ApiResult<TripSummary>> {
  return apiRequest<ApiResult<TripSummary>>('/trips', { method: 'POST', body: input });
}