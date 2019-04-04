import ReactGA from 'react-ga';

let initialised = false;

if (process.env.GA_TRACKING_ID !== undefined && process.env.GA_TRACKING === 'true') {
  ReactGA.initialize(process.env.GA_TRACKING_ID);
  initialised = true;

  // tslint:disable-next-line:no-console
  console.log('GA initialised');
}

export const logPageView = () => {
  if (initialised) {
    ReactGA.set({ page: window.location.pathname });
    ReactGA.pageview(window.location.pathname);
  }
};

export const logEvent = (category: string, action: string) => {
  if (initialised) {
    ReactGA.event({ category, action });
  }
};

export const logException = (description: string, fatal: boolean = false) => {
  if (initialised) {
    ReactGA.exception({ description, fatal });
  }
};
