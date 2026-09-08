declare module 'memoizee/weak' {
  import * as memoizee from 'memoizee';

  // eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
  export default function <F extends Function>(f: F): F & memoizee.Memoized<F>;
}
