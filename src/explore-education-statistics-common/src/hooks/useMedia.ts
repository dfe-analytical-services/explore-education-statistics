import { useEffect, useState } from 'react';

export const breakpoints = {
  mobile: '320px',
  tablet: '641px',
  desktop: '769px',
};

/**
 * Hook that detects if the window currently
 * matches a given media {@param query} string.
 */
export default function useMedia(query: string) {
  const [isMedia, setMedia] = useState(false);

  useEffect(() => {
    let mounted = true;

    const onChange = (event: MediaQueryListEvent) => {
      if (!mounted) {
        return;
      }

      setMedia(event.matches);
    };

    const mediaQueryList = window.matchMedia(query);
    setMedia(mediaQueryList.matches);

    mediaQueryList.addListener(onChange);

    return () => {
      mounted = false;
      mediaQueryList.removeListener(onChange);
    };
  }, [query]);

  return {
    isMedia,
    onMedia<T, R>(value: T, defaultValue?: R): T | R | undefined {
      return isMedia ? value : defaultValue;
    },
  };
}

export const useMobileMedia = () =>
  useMedia(`(max-width: ${breakpoints.tablet})`);

export const useDesktopMedia = () =>
  useMedia(`(min-width: ${breakpoints.desktop})`);
