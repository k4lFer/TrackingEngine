import type { ApiMessage, ApiPagedResult, ApiResult } from '../types';
import type { LoginResponse } from '../auth';
import { getDeviceId, setSession } from '../../storage/session';

/** Config global de la API (en dev el backend corre en Docker y se expone en 8080). */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080';

/** URL del hub de SignalR (en producción queda en el mismo origen). */
export const TRACKING_HUB_URL = API_BASE_URL + '/hubs/tracking';

const HEADER_AUTH = 'Authorization';
const HEADER_DEVICE_ID = 'X-Device-Id';

/**
 * Error que envuelve una respuesta con estado HTTP no 2xx, incluyendo el
 * detalle de mensajes que devuelve el backend en el patrón OutputPort.
 */
export class ApiHttpError extends Error {
  readonly status: number;
  readonly messages: ApiMessage[];

  constructor(status: number, messages: ApiMessage[] = []) {
    const first = messages[0];
    super(first?.message ?? `HTTP ${status}`);
    this.name = 'ApiHttpError';
    this.status = status;
    this.messages = messages;
  }
}

export interface ApiRequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  /** Params de query; se serializan y se ignoran si son null/undefined/''. */
  query?: Record<string, unknown>;
  headers?: Record<string, string>;
  /** Se envía como JSON. */
  body?: unknown;
  /**
   * 'auto' (defecto): inyecta el JWT (Authorization) y el X-Device-Id si hay
   * sesión activa y maneja 401 refrescando el token automáticamente.
   * 'none': sin credenciales (endpoints públicos).
   */
  auth?: 'auto' | 'none';
  signal?: AbortSignal;
  /** Usado internamente para reintentar una petición tras refrescar el token. */
  _retried?: boolean;
}

function toQueryString(query: Record<string, unknown>): string {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(query)) {
    if (value === null || value === undefined || value === '') continue;
    params.set(key, String(value));
  }
  const str = params.toString();
  return str ? `?${str}` : '';
}

/**
 * Asegura el prefijo `/api` de las rutas REST del backend: los dominios
 * llaman paths como `/vehicles` o `/geofences` y auth/user ya envían
 * `/api/auth/...`, así que aquí se normaliza una sola vez.
 */
function apiPath(path: string): string {
  return path.startsWith('/api') ? path : `/api${path}`;
}

/** Deserializa el cuerpo según content-type (JSON o texto). */
async function parseBody<T>(res: Response): Promise<T | null> {
  if (res.status === 204) return null;
  const contentType = res.headers.get('content-type') ?? '';
  if (contentType.includes('application/json')) {
    return (await res.json()) as T;
  }
  return (await res.text()) as T;
}

type AuthHeaders = {
  accessToken: string | null;
  deviceId: string;
};

/** Callbacks que el AuthProvider registra al arrancar (evita imports cíclicos). */
type ClientHooks = {
  getSessionTokens: () => AuthHeaders;
  onRefreshed: (session: LoginResponse) => void;
  onUnauthorized: () => void;
};

let hooks: ClientHooks = {
  getSessionTokens: () => ({ accessToken: null, deviceId: getDeviceId() }),
  onRefreshed: (session) => setSession(session),
  onUnauthorized: () => {},
};

/** Registra los callbacks de sesión usados por el interceptor. */
export function configureClient(next: ClientHooks): void {
  hooks = next;
}

/** Promise compartida para no lanzar múltiples refrescos en paralelo. */
let refreshInFlight: Promise<string | null> | null = null;

/**
 * Refresca el token de acceso de forma silenciosa y devuelve el nuevo JWT.
 * El refresh token viaja en una HttpOnly cookie, así que esta petición no
 * necesita body ni cabeceras adicionales: basta con credentials: 'include'.
 */
async function refreshAccessToken(): Promise<string | null> {
  if (refreshInFlight) return refreshInFlight;

  refreshInFlight = (async () => {
    try {
      const res = await fetch(`${API_BASE_URL}/api/auth/refresh-token`, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      if (!res.ok) return null;

      const payload = (await res.json()) as ApiResult<LoginResponse>;
      if (!payload.data) return null;

      hooks.onRefreshed(payload.data);
      return payload.data.token;
    } catch {
      return null;
    } finally {
      refreshInFlight = null;
    }
  })();

  return refreshInFlight;
}

/**
 * Interceptor HTTP sobre fetch:
 * - inyecta Authorization (Bearer) y X-Device-Id en modo 'auto'
 * - ante un 401 intenta refrescar el token una vez y reintenta la petición
 * - si el refresco falla notifica a través de onUnauthorized (sesión expirada)
 * - traduce la respuesta (patrón OutputPort) a valor tipado o ApiHttpError
 */
export async function apiRequest<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const { method = 'GET', query, headers, body, auth = 'auto', signal, _retried } = options;

  const url = `${API_BASE_URL}${apiPath(path)}${query ? toQueryString(query) : ''}`;

  const authHeaders: Record<string, string> = {};
  if (auth === 'auto') {
    const session = hooks.getSessionTokens();
    if (session.accessToken) authHeaders[HEADER_AUTH] = `Bearer ${session.accessToken}`;
    authHeaders[HEADER_DEVICE_ID] = session.deviceId;
  }

  const finalHeaders: Record<string, string> = {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    ...authHeaders,
    ...headers,
  };

  const res = await fetch(url, {
    method,
    headers: finalHeaders,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    credentials: 'include',
    signal,
  });

  // Interceptor de 401: intenta refrescar y reintenta una sola vez.
  if (res.status === 401 && auth === 'auto' && !_retried && path !== '/api/auth/refresh-token') {
    const newToken = await refreshAccessToken();
    if (newToken) {
      return apiRequest<T>(path, { ...options, _retried: true });
    }
    hooks.onUnauthorized();
  }

  const payload = await parseBody<{ messages?: ApiMessage[] }>(res);

  if (!res.ok) {
    throw new ApiHttpError(res.status, payload?.messages ?? []);
  }

  return payload as T;
}

/**
 * GET de listado que normaliza el envelope del backend: los endpoints de
 * listado devuelven `data` como QueryResult paginado `{ results, ... }` (p. ej.
 * /vehicles, /geofences) o directamente como array (p. ej. /trips). Aquí se
 * desenvuelve de forma central a `ApiResult<T[]>` para que cada dominio reciba
 * siempre la lista tipada.
 */
export async function apiList<T>(
  path: string,
  options: Omit<ApiRequestOptions, 'method'> = {},
): Promise<ApiResult<T[]>> {
  const payload =
    await apiRequest<ApiPagedResult<T> | ApiResult<T[]>>(path, { ...options, method: 'GET' });
  const raw = payload?.data;
  const items = Array.isArray(raw) ? raw : (raw?.results ?? []);
  return { data: items, messages: payload?.messages ?? [] };
}

/** ApiResult<T[]> con los metadatos de paginación del backend (para tablas admin). */
export interface PagedList<T> extends ApiResult<T[]> {
  totalCount: number;
  totalPages: number;
  pageNumber: number;
  pageSize: number;
}

/**
 * Variante de apiList que además expone totalCount/totalPages/pageNumber/pageSize
 * del QueryResult<T> del backend, necesarios para pintar el componente Pagination.
 */
export async function apiListPaged<T>(
  path: string,
  options: Omit<ApiRequestOptions, 'method'> = {},
): Promise<PagedList<T>> {
  const payload =
    await apiRequest<ApiPagedResult<T> | ApiResult<T[]>>(path, { ...options, method: 'GET' });
  const raw = payload?.data;
  const items = (Array.isArray(raw) ? raw : raw?.results) ?? [];
  const paged = !Array.isArray(raw) && raw ? raw : null;
  return {
    data: items,
    messages: payload?.messages ?? [],
    totalCount: paged?.totalCount ?? items.length,
    totalPages: paged?.totalPages ?? (items.length === 0 ? 0 : 1),
    pageNumber: paged?.pageNumber ?? 1,
    pageSize: paged?.pageSize ?? items.length,
  };
}