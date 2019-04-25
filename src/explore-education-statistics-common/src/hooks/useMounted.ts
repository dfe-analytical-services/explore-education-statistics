import { useEffect, useState } from 'react';

/**
 * Simple hook that returns true once a component
 * has mounted.
 *
 * This is particularly useful for progressively
 * enhancing components in SSR where not all attributes
 * should be set until the client-side JS loads.
 */
function useMounted() {
  const [isMounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  return {
    isMounted,
    onMounted<T, R>(value: T, defaultValue?: R): T | R | undefined {
      return isMounted ? value : defaultValue;
    },
  };
}

export default useMounted;
