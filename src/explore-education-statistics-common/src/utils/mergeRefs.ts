import { RefCallback, MutableRefObject } from 'react';

type MutableRefList<T> = Array<
  RefCallback<T> | MutableRefObject<T> | undefined | null
>;

export function mergeRefs<T>(...refs: MutableRefList<T>): RefCallback<T> {
  return (val: T) => {
    setRef(val, ...refs);
  };
}

export function setRef<T>(val: T, ...refs: MutableRefList<T>): void {
  refs.forEach(ref => {
    if (typeof ref === 'function') {
      ref(val);
    } else if (ref != null) {
      // eslint-disable-next-line no-param-reassign
      ref.current = val;
    }
  });
}
