import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import client from '@admin/services/util/service';

const service = {
  GetInsightsKey(): Promise<string> {
    return client.get<string>('/configuration/application-insights');
  },
};

// eslint-disable-next-line import/prefer-default-export
export const initApplicationInsights = async () => {
  const key = await service.GetInsightsKey();
  if (key) {
    const appInsights = new ApplicationInsights({
      config: {
        instrumentationKey: key,
        enableAutoRouteTracking: true,
        autoTrackPageVisitTime: true,
      },
    });
    appInsights.loadAppInsights();

    console.log('Application Insights initialised');
  }
};