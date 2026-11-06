import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { CreateVehicleInput, Vehicle } from './types';

/** POST /vehicles — alta de una unidad. */
export function create(input: CreateVehicleInput): Promise<ApiResult<Vehicle>> {
  return apiRequest<ApiResult<Vehicle>>('/vehicles', {
    method: 'POST',
    body: input,
  });
}