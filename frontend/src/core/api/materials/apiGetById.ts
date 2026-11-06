import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { Material } from './types';

/** GET /materials/{id} — detalle de un material. */
export function getById(id: string): Promise<ApiResult<Material>> {
  return apiRequest<ApiResult<Material>>(`/materials/${id}`);
}