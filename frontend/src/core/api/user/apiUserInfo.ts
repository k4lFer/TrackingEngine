import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { ProfileResponse } from './apiProfile';

/** GET /api/user/{id} — autenticado con JWT (interceptor). */
export function getUserInfo(id: string): Promise<ApiResult<ProfileResponse>> {
  return apiRequest<ApiResult<ProfileResponse>>(`/api/user/${id}`, {
    method: 'GET',
  });
}