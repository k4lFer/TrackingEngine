import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** Entrada de POST /api/auth/sign-up (refleja SignUpDto del backend). */
export interface SignUpInput {
  email: string;
  username: string;
  password: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string; // ISO (yyyy-MM-dd)
}

/** POST /api/auth/sign-up */
export function signUp(input: SignUpInput): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>('/api/auth/sign-up', {
    method: 'POST',
    body: input,
    auth: 'none',
  });
}