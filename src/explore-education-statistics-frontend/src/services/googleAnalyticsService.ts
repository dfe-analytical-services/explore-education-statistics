import ReactGA from 'react-ga4';

let initialised = false;
let gaTrackingId = '';

export const googleAnalyticsCookies = ['_ga', '_gid', '_gat'];

export function initGoogleAnalytics(trackingId: string) {
  if (!initialised && trackingId) {
    ReactGA.initialize(trackingId);

    gaTrackingId = trackingId;
    initialised = true;

    // eslint-disable-next-line no-console
    console.log('GA initialised');
  }
}

export function enableGoogleAnalytics() {
  // eslint-disable-next-line @typescript-eslint/ban-ts-comment
  // @ts-ignore
  window[`ga-disable-${gaTrackingId}`] = false;
}
export function disableGoogleAnalytics() {
  // eslint-disable-next-line @typescript-eslint/ban-ts-comment
  // @ts-ignore
  window[`ga-disable-${gaTrackingId}`] = true;
}

export const logPageView = () => {
  if (initialised) {
    ReactGA.send({ hitType: 'pageview' });
  }
};

export interface AnalyticsEvent {
  category: string;
  action: string;
  label?: string;
  value?: number;
}

export function logEvent({ category, action, label, value }: AnalyticsEvent) {
  if (!initialised) {
    return;
  }

  ReactGA.event({
    category,
    action,
    label,
    value,
  });
}
