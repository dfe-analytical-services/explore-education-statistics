import {
  ApplicationInsights,
  SeverityLevel,
} from '@microsoft/applicationinsights-web';

export { ApplicationInsights, SeverityLevel };

let isInitialised = false;

const appInsights = new ApplicationInsights({
  config: {
    autoTrackPageVisitTime: true,
  },
});

export function initApplicationInsights(
  instrumentationKey: string,
): ApplicationInsights {
  if (instrumentationKey) {
    appInsights.config.instrumentationKey = instrumentationKey;

    if (!isInitialised) {
      appInsights.loadAppInsights();
      isInitialised = true;

      // eslint-disable-next-line no-console
      console.log('Application Insights initialised');
    }
  }

  return appInsights;
}

/**
 * Escape hatch to get the Application Insights client
 * directly. Prefer to use context if possible.
 */
export function getApplicationInsights(): ApplicationInsights {
  return appInsights;
}
