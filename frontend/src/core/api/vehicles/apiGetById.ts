import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { Vehicle } from './types';

/** GET /vehicles/{id} — detalle de una unidad. */
export function getById(id: string): Promise<ApiResult<Vehicle>> {
  return apiRequest<ApiResult<Vehicle>>(`/vehicles/${id}`);
}