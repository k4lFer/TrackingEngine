import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { UpdateVehicleInput, Vehicle } from './types';

/** PUT /vehicles/{id} — actualización de una unidad. */
export function update(id: string, input: UpdateVehicleInput): Promise<ApiResult<Vehicle>> {
  return apiRequest<ApiResult<Vehicle>>(`/vehicles/${id}`, {
    method: 'PUT',
    body: input,
  });
}