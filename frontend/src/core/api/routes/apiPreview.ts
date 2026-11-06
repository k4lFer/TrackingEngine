import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { RouteItem, RoutePreviewInput } from './types';

/** POST /routes/preview — calcula la ruta (Valhalla) sin persistirla. */
export function preview(input: RoutePreviewInput): Promise<ApiResult<RouteItem>> {
  return apiRequest<ApiResult<RouteItem>>('/routes/preview', {
    method: 'POST',
    body: input,
  });
}