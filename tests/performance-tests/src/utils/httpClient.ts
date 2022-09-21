import http, { RefinedParams, RefinedResponse, RequestBody } from 'k6/http';

interface HttpHeaders {
  [name: string]: string;
}

export default class HttpClient {
  private readonly baseUrl: string;

  private readonly accessToken?: string;

  private readonly checkResponseStatus: boolean;

  constructor({
    baseUrl,
    accessToken,
    checkResponseStatus = true,
  }: {
    baseUrl: string;
    accessToken?: string;
    checkResponseStatus?: boolean;
  }) {
    this.baseUrl = baseUrl;
    this.accessToken = accessToken;
    this.checkResponseStatus = checkResponseStatus;
  }

  get<TJson>(
    url: string,
    headers?: HttpHeaders,
    additionalAllowedHttpCodes?: number[],
  ): {
    json: TJson;
    response: RefinedResponse<'text'>;
  } {
    const params = HttpClient.getDefaultParams(this.accessToken);
    const response = http.get(`${this.baseUrl}${url}`, {
      ...params,
      headers: {
        ...params.headers,
        ...headers,
      },
    });

    const successCodes = [200, ...(additionalAllowedHttpCodes ?? [])];

    if (this.checkResponseStatus && !successCodes.includes(response.status)) {
      throw new Error(
        `${response.status} error with GET to url ${this.baseUrl}${url}: ${response.body}`,
      );
    }

    return {
      json: (response.status === 200
        ? response.json()
        : ({} as unknown)) as TJson,
      response,
    };
  }

  post<TJson>(
    url: string,
    data: RequestBody | string,
    headers?: HttpHeaders,
  ): {
    json: TJson;
    response: RefinedResponse<'text'>;
  } {
    const params = HttpClient.getDefaultParams(this.accessToken);
    const response = http.post(`${this.baseUrl}${url}`, data, {
      ...params,
      headers: {
        ...params.headers,
        ...headers,
      },
    });

    if (this.checkResponseStatus && response.status !== 200) {
      throw new Error(
        `${response.status} error with POST to url ${this.baseUrl}${url} with request body\n\n${data}: ${response.body}`,
      );
    }

    return {
      json: (response.json() as unknown) as TJson,
      response,
    };
  }

  patch<TJson>(
    url: string,
    data: RequestBody | string,
    headers?: HttpHeaders,
  ): {
    json: TJson;
    response: RefinedResponse<'text'>;
  } {
    const params = HttpClient.getDefaultParams(this.accessToken);
    const response = http.patch(`${this.baseUrl}${url}`, data, {
      ...params,
      headers: {
        ...params.headers,
        ...headers,
      },
    });

    if (this.checkResponseStatus && response.status !== 200) {
      throw new Error(
        `${response.status} error with PATCH to url ${this.baseUrl}${url} with request body\n\n${data}: ${response.body}`,
      );
    }

    return {
      json: (response.json() as unknown) as TJson,
      response,
    };
  }

  delete(
    url: string,
    data?: RequestBody | string,
    headers?: HttpHeaders,
  ): RefinedResponse<'text'> {
    const params = HttpClient.getDefaultParams(this.accessToken);
    const response = http.del(`${this.baseUrl}${url}`, data, {
      ...params,
      headers: {
        ...params.headers,
        ...headers,
      },
    });

    if (this.checkResponseStatus && response.status !== 204) {
      throw new Error(
        `Error with DELETE to url ${this.baseUrl}${url}: ${response.body}`,
      );
    }
    return response;
  }

  private static getDefaultParams(accessToken?: string): RefinedParams<'text'> {
    return {
      timeout: '300s',
      headers: {
        ...(accessToken
          ? {
              Authorization: `Bearer ${accessToken}`,
            }
          : {}),
      },
    };
  }
}
