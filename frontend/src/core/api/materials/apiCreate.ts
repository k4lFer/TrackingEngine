import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { CreateMaterialInput, Material } from './types';

/** POST /materials — alta de un material. */
export function create(input: CreateMaterialInput): Promise<ApiResult<Material>> {
  return apiRequest<ApiResult<Material>>('/materials', {
    method: 'POST',
    body: input,
  });
}