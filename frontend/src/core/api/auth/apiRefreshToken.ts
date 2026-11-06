import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { LoginResponse } from './apiLogin';

/**
 * POST /api/auth/refresh-token
 * El refresh token se lee de la HttpOnly cookie (se envía solo con credentials),
 * por eso este endpoint no recibe body.
 */
export function refreshToken(): Promise<ApiResult<LoginResponse>> {
  return apiRequest<ApiResult<LoginResponse>>('/api/auth/refresh-token', {
    method: 'POST',
    auth: 'none',
  });
}