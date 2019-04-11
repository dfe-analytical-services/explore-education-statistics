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
    onRendered(value: any, unRenderedValue?: any) {
      return isRendered ? value : unRenderedValue;
    },
  };
}

export default useRendered;
