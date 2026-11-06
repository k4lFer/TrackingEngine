import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** Refleja ProfileResponseDto del backend. */
export interface ProfileResponse {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  registerDate: string;
}

/** GET /api/user/profile — autenticado con JWT (interceptor). */
export function getMyProfile(): Promise<ApiResult<ProfileResponse>> {
  return apiRequest<ApiResult<ProfileResponse>>('/api/user/profile', {
    method: 'GET',
  });
}