import { RefObject, useEffect, useState } from 'react';

/**
 * Hook to create an intersection observer for an
 * element and return any observation entries.
 * Useful for detecting visibility changes to the element.
 */
export default function useIntersectionObserver(
  ref: RefObject<HTMLElement>,
  options: IntersectionObserverInit,
): IntersectionObserverEntry[] {
  const [entries, setEntries] = useState<IntersectionObserverEntry[]>([]);

  useEffect(() => {
    setEntries([]);

    const observer = new IntersectionObserver(setEntries, options);

    if (ref.current) {
      observer.observe(ref.current);
    }

    return () => {
      setEntries([]);
      observer.disconnect();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [JSON.stringify(options), ref]);

  return entries;
}
