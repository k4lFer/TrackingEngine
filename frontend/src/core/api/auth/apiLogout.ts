import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/**
 * POST /api/auth/logout — revoca la sesión del dispositivo actual
 * (identificado por X-Device-Id) y borra la HttpOnly cookie en el servidor.
 */
export function logout(): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>('/api/auth/logout', {
    method: 'POST',
  });
}