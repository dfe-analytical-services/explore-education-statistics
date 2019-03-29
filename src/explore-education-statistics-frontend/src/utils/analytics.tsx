import ReactGA from 'react-ga';

export const initGA = () => {
  const trackingId = process.env.GA_TRACKING_ID;

  if (trackingId !== undefined) {
    // tslint:disable-next-line:no-console
    console.log('GA init');
    ReactGA.initialize(trackingId);
  }
};

export const logPageView = () => {
  // tslint:disable-next-line:no-console
  console.log(`Logging pageview for ${window.location.pathname}`);
  ReactGA.set({ page: window.location.pathname });
  ReactGA.pageview(window.location.pathname);
};

export const logEvent = (category = '', action = '') => {
  if (category && action) {
    ReactGA.event({ category, action });
  }
};

export const logException = (description = '', fatal = false) => {
  if (description) {
    ReactGA.exception({ description, fatal });
  }
};
