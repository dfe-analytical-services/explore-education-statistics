import ReactGA from 'react-ga';
import {
  getCookies,
  deleteCookies,
  cookieSettingsCookieName,
} from '@frontend/services/cookiesService';

let initialised = false;

const googleAnalyticsCookies = ['_ga', '_gid', '_gat'];

export function initGA() {
  if (
    process.env.GA_TRACKING_ID !== undefined &&
    process.env.GA_TRACKING === 'true' &&
    !initialised &&
    // @ts-ignore
    getCookies()[cookieSettingsCookieName]
  ) {
    ReactGA.initialize(process.env.GA_TRACKING_ID);
    initialised = true;

    // eslint-disable-next-line no-console
    console.log('GA initialised');
  }
}
export function enableGA() {
  console.log('enableGA');
  // @ts-ignore
  window[`ga-disable-${process.env.GA_TRACKING_ID}`] = false;
  initGA();
}
export function disableGA() {
  console.log('disableGA');
  // @ts-ignore
  window[`ga-disable-${process.env.GA_TRACKING_ID}`] = true;
  deleteCookies(googleAnalyticsCookies);
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

export const logException = (description: string, fatal: boolean = false) => {
  if (initialised) {
    ReactGA.exception({ description, fatal });
  }
};

export interface AnalyticProps {
  analytics?: {
    category: string;
    action: string;
  };
}
