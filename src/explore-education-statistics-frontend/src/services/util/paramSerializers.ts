interface Params {
  [param: string]: string | number | ArrayLike<string> | ArrayLike<number>;
}

export const noBrackets = (
  params: Params,
  encode: (input: string) => string = encodeURIComponent,
) =>
  Object.entries(params)
    .map(([param, value]) => {
      if (Array.isArray(value)) {
        return value.map(item => `${encode(param)}=${encode(item)}`).join('&');
      }

      return `${encode(param)}=${encode(`${value}`)}`;
    })
    .join('&');

export const commaSeparated = (
  params: Params,
  encode: (input: string) => string = encodeURIComponent,
) =>
  Object.entries(params)
    .map(([param, value]) => {
      if (Array.isArray(value)) {
        return `${encode(param)}=${value.map(item => encode(item)).join(',')}`;
      }

      return `${encode(param)}=${encode(`${value}`)}`;
    })
    .join('&');
