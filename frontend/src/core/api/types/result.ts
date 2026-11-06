/** Refleja el patrón de MessageDto del backend (App.Shared.Result.MessageDto). */
export interface ApiMessage {
  code: string;
  message: string;
}

/** Refleja el patrón de OutputPort<T> del backend (App.Shared.Result.OutputPort<T>). */
export type ApiResult<T> = {
  data: T | null;
  messages: ApiMessage[];
};