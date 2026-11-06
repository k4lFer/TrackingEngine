import type { LoginResponse } from '../api/auth';
import { readStorage, removeStorage, writeStorage } from './index';

const KEY_SESSION = 'session';
const KEY_DEVICE_ID = 'deviceId';

/** Persiste la sesión (access token + datos del usuario). El refresh token vive en HttpOnly cookie. */
export function getSession(): LoginResponse | null {
  return readStorage<LoginResponse | null>(KEY_SESSION, null);
}

export function setSession(session: LoginResponse): void {
  writeStorage(KEY_SESSION, session);
}

export function clearSession(): void {
  removeStorage(KEY_SESSION);
}

/** Token de acceso actual (Bearer). */
export function getAccessToken(): string | null {
  return getSession()?.token ?? null;
}

/** Device ID estable de este navegador (X-Device-Id). */
export function getDeviceId(): string {
  const existing = readStorage<string | null>(KEY_DEVICE_ID, null);
  if (existing) return existing;
  const generated = crypto.randomUUID();
  writeStorage(KEY_DEVICE_ID, generated);
  return generated;
}