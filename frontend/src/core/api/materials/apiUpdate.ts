import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { Material, UpdateMaterialInput } from './types';

/** PUT /materials/{id} — actualización de un material. */
export function update(id: string, input: UpdateMaterialInput): Promise<ApiResult<Material>> {
  return apiRequest<ApiResult<Material>>(`/materials/${id}`, {
    method: 'PUT',
    body: input,
  });
}