export const noBrackets = (
  params: {
    [param: string]: string | number | ArrayLike<string> | ArrayLike<number>;
  },
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
