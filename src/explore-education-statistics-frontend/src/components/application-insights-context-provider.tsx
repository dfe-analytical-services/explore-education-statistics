'use client';

import {
  ApplicationInsightsContextProvider as BaseApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import React, { ReactNode, useEffect } from 'react';
import { usePathname, useSearchParams } from 'next/navigation';

export default function ApplicationInsightsContextProvider({
  children,
}: {
  children?: ReactNode;
}) {
  return (
    <BaseApplicationInsightsContextProvider
      instrumentationKey={process.env.APPINSIGHTS_INSTRUMENTATIONKEY}
    >
      {children}
    </BaseApplicationInsightsContextProvider>
  );
}

export const ApplicationInsightsTracking = () => {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const appInsights = useApplicationInsights();

  useEffect(() => {
    if (!appInsights) {
      return;
    }

    appInsights.trackPageView({
      uri: pathname ?? undefined,
    });

    const uri = `${pathname}?${searchParams}`;
    appInsights.trackPageView({ uri });
  }, [appInsights, pathname, searchParams]);

  return null;
};
