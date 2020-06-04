import ReactGA from 'react-ga';

let initialised = false;

export const googleAnalyticsCookies = ['_ga', '_gid', '_gat'];

export function initGoogleAnalytics(trackingId: string) {
  if (!initialised) {
    ReactGA.initialize(trackingId);
    initialised = true;

    // eslint-disable-next-line no-console
    console.log('GA initialised');
  }
}

export function enableGoogleAnalytics() {
  // @ts-ignore
  window[`ga-disable-${process.env.GA_TRACKING_ID}`] = false;
}
export function disableGoogleAnalytics() {
  // @ts-ignore
  window[`ga-disable-${process.env.GA_TRACKING_ID}`] = true;
}

export const logPageView = () => {
  if (initialised) {
    ReactGA.set({ page: window.location.pathname });
    ReactGA.pageview(window.location.pathname);
  }
};

export const logEvent = (
  category: string,
  action: string,
  label?: string,
  value?: number,
) => {
  if (initialised) {
    ReactGA.event({ category, action, label, value });
  }
};

export const logException = (description: string, fatal = false) => {
  if (initialised) {
    ReactGA.exception({ description, fatal });
  }
};

export interface AnalyticProps {
  analytics?: {
    category: string;
    action: string;
    label?: string;
  };
}
