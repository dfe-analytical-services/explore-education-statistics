import { ApplicationInsights } from '@microsoft/applicationinsights-web';

// eslint-disable-next-line import/prefer-default-export
export function initApplicationInsights(
  key: string,
): ApplicationInsights | undefined {
  if (key) {
    const appInsights = new ApplicationInsights({
      config: {
        instrumentationKey: key,
        enableAutoRouteTracking: true,
        autoTrackPageVisitTime: true,
      },
    });
    appInsights.loadAppInsights();

    // eslint-disable-next-line no-console
    console.log('Application Insights initialised');

    return appInsights;
  }

  return undefined;
}
