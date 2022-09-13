import useToggle from '@common/hooks/useToggle';
import { useEffect } from 'react';
import { useRouter } from 'next/router';

export default function useRouterLoading(): boolean {
  const router = useRouter();
  const [isLoading, toggleLoading] = useToggle(false);

  useEffect(() => {
    router.events.on('routeChangeStart', toggleLoading.on);
    router.events.on('routeChangeComplete', toggleLoading.off);

    return () => {
      router.events.off('routeChangeStart', toggleLoading.on);
      router.events.off('routeChangeComplete', toggleLoading.off);
    };
  }, [toggleLoading, router]);

  return isLoading;
}
