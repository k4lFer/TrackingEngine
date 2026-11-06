import type { ApiResult } from './result';

/** Refleja el patrón QueryResult<T> del backend (App.Shared.Query.QueryResult<T>). */
export interface ApiQueryResult<T> {
  results: T[];
  totalCount: number;
  totalPages: number;
  pageNumber: number;
  pageSize: number;
}

/** Forma del cuerpo de una respuesta paginada del backend (OutputPort<QueryResult<T>>). */
export type ApiPagedResult<T> = ApiResult<ApiQueryResult<T>>;