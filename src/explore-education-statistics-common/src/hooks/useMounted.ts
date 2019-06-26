import { EffectCallback, useEffect, useState } from 'react';

/**
 * Simple hook that returns true once a component
 * has mounted.
 *
 * This is particularly useful for progressively
 * enhancing components in SSR where not all attributes
 * should be set until the client-side JS loads.
 */
function useMounted(effect?: EffectCallback) {
  const [isMounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);

    if (effect) {
      effect();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return {
    isMounted,
    onMounted<T, R>(value: T, defaultValue?: R): T | R | undefined {
      return isMounted ? value : defaultValue;
    },
  };
}

export default useMounted;
