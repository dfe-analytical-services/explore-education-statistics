import ReactGA from 'react-ga';

let initialised = false;

export const googleAnalyticsCookies = ['_ga', '_gid', '_gat'];

if (process.env.GA_TRACKING_ID && !initialised) {
  ReactGA.initialize(process.env.GA_TRACKING_ID);
  initialised = true;

  // eslint-disable-next-line no-console
  console.log('GA initialised');
}

export function enableGA() {
  // @ts-ignore
  window[`ga-disable-${process.env.GA_TRACKING_ID}`] = false;
}
export function disableGA() {
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
