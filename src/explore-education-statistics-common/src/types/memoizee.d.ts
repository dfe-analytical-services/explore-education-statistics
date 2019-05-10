declare module 'memoizee/weak' {
  import * as memoizee from 'memoizee';

  export default function<F extends Function>(f: F): F & memoizee.Memoized<F>;
}
