import { NextApiRequest, NextApiResponse } from 'next';
import {
  createMocks,
  createRequest,
  createResponse,
  Mocks,
  MockRequest,
  MockResponse,
  RequestOptions,
  ResponseOptions,
} from 'node-mocks-http';

type ApiRequest = NextApiRequest & ReturnType<typeof createRequest>;
type ApiResponse = NextApiResponse & ReturnType<typeof createResponse>;

export function createApiMocks(
  reqOptions?: RequestOptions,
  resOptions?: ResponseOptions,
): Mocks<ApiRequest, ApiResponse> {
  return createMocks(reqOptions, resOptions);
}

export function createApiRequest(
  reqOptions?: RequestOptions,
): MockRequest<ApiRequest> {
  return createRequest(reqOptions);
}

export function createApiResponse(
  resOptions?: ResponseOptions,
): MockResponse<ApiResponse> {
  return createResponse(resOptions);
}
