'use client';

import { useEffect } from 'react';
import { usePathname, useSearchParams } from 'next/navigation';
import { useCookies } from '@frontend/hooks/useCookies';
import { Dictionary } from '@common/types';
import useMounted from '@common/hooks/useMounted';

interface Props {
  cookies: Dictionary<string>;
}

export default function NavigationEvents({ cookies }: Props) {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const { getCookie } = useCookies(cookies);

  let onRouteChangeCallback: () => void = () => undefined;

  // TODO: Do we still need this within a 'use client' component? Probably not
  useMounted(() => {
    if (process.env.GA_TRACKING_ID && getCookie('disableGA') !== 'true') {
      import('@frontend/services/googleAnalyticsService').then(
        ({ initGoogleAnalytics, logPageView }) => {
          initGoogleAnalytics(process.env.GA_TRACKING_ID);

          onRouteChangeCallback = logPageView;
        },
      );
    }

    document.body.classList.add('js-enabled', 'govuk-frontend-supported');
  });

  useEffect(() => {
    onRouteChangeCallback(); // logPageView()
  }, [pathname, searchParams]);

  return null;
}
