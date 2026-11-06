import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** Entrada de POST /api/auth/login (refleja LoginDto del backend). */
export interface LoginInput {
  email: string;
  password: string;
  /** Si no se envía, el backend genera uno. */
  deviceId?: string;
}

/** Respuesta de login y refresh (refleja LoginResponseDto del backend). */
export interface LoginResponse {
  token: string;
  userId: string;
  email: string;
  username: string;
  deviceId: string;
}

/** POST /api/auth/login — devuelve el access token; el refresh token viaja en HttpOnly cookie. */
export function login(input: LoginInput): Promise<ApiResult<LoginResponse>> {
  return apiRequest<ApiResult<LoginResponse>>('/api/auth/login', {
    method: 'POST',
    body: input,
    auth: 'none',
  });
}