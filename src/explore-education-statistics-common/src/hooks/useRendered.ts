import { useEffect, useState } from 'react';

/**
 * Simple hook that returns true once a component
 * has rendered.
 *
 * This is particularly useful for progressively
 * enhancing components in SSR where not all attributes
 * should be set until the client-side JS loads.
 */
function useRendered() {
  const [isRendered, setRendered] = useState(false);

  useEffect(() => {
    setRendered(true);
  });

  return {
    isRendered,
    onRendered<T, R>(value: T, defaultValue?: R): T | R | undefined {
      return isRendered ? value : defaultValue;
    },
  };
}

export default useRendered;
