import http, { RefinedResponse, RequestBody } from 'k6/http';

type HttpHeaders = { [name: string]: string };

export default function createClient(
  baseUrl: string,
  accessToken: string,
  checkResponseStatus = true,
) {
  return {
    get: <TJson>(
      url: string,
      headers?: HttpHeaders,
    ): {
      json: TJson;
      response: RefinedResponse<'text'>;
    } => {
      const response = http.get(
        `${baseUrl}${url}`,
        getHttpHeaders(accessToken, headers),
      );

      if (checkResponseStatus && response.status !== 200) {
        throw new Error(`Error with GET to url ${url}: ${response.body}`);
      }

      return {
        json: (response.json() as unknown) as TJson,
        response,
      };
    },

    post: <TJson>(
      url: string,
      data: RequestBody | string,
      headers?: HttpHeaders,
    ): {
      json: TJson;
      response: RefinedResponse<'text'>;
    } => {
      const response = http.post(
        `${baseUrl}${url}`,
        data,
        getHttpHeaders(accessToken, headers),
      );

      if (checkResponseStatus && response.status !== 200) {
        throw new Error(`Error with POST to url ${url}: ${response.body}`);
      }

      return {
        json: (response.json() as unknown) as TJson,
        response,
      };
    },

    delete: (
      url: string,
      data?: RequestBody | string,
      headers?: HttpHeaders,
    ): RefinedResponse<'text'> => {
      const response = http.del(
        `${baseUrl}${url}`,
        data,
        getHttpHeaders(accessToken, headers),
      );

      if (checkResponseStatus && response.status !== 204) {
        throw new Error(`Error with DELETE to url ${url}: ${response.body}`);
      }

      return response;
    },
  };
}

export function getHttpHeaders(accessToken: string, headers?: HttpHeaders) {
  return {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      ...headers,
    },
  };
}
