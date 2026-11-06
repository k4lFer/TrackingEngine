const PREFIX = 'backendmodel.';

/** Lee un valor JSON de localStorage. */
export function readStorage<T>(key: string, fallback: T): T {
  try {
    const raw = localStorage.getItem(PREFIX + key);
    if (raw === null) return fallback;
    return JSON.parse(raw) as T;
  } catch {
    return fallback;
  }
}

/** Escribe un valor JSON en localStorage. */
export function writeStorage(key: string, value: unknown): void {
  localStorage.setItem(PREFIX + key, JSON.stringify(value));
}

/** Elimina una clave de localStorage. */
export function removeStorage(key: string): void {
  localStorage.removeItem(PREFIX + key);
}