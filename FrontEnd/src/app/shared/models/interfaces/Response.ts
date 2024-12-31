export interface IResponse {
  statusCode: number;
  message?: string;
  stackTrace?: string;
}
export interface IPostResponse extends IResponse {
  id: number;
}
export interface ApiResponse {
  statusCode: number;
  message: string;
  id: number;
}
