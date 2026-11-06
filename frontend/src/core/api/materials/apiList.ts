import { apiListPaged, type PagedList } from '../http/client';
import type { Material } from './types';

/** Query de GET /materials (refleja MaterialFilterDto del backend). */
export interface MaterialQuery {
  NumberPage?: number;
  PageSize?: number;
  Search?: string;
  Active?: boolean;
}

/** GET /materials — listado paginado + metadatos para el paginador. */
export function list(query: MaterialQuery = {}): Promise<PagedList<Material>> {
  return apiListPaged<Material>('/materials', { query: { ...query } });
}