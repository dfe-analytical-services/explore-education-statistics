import ReactGA from 'react-ga';

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
    ReactGA.set({ page: window.location.pathname });
    ReactGA.pageview(window.location.pathname);
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

export function logOutboundLink(label: string, url: string, newTab?: boolean) {
  // we don't want to attempt to log outbound links if the user has disabled analytics
  // but we still want any links that are external to open in a new tab

  function openLink() {
    if (newTab) {
      window.open(url, '_blank');
      return;
    }
    document.location.href = url;
  }

  if (!initialised) {
    openLink();
    return;
  }

  ReactGA.outboundLink(
    {
      label,
    },

    function hitCallback() {
      openLink();
    },
  );
}

export const logException = (description: string, fatal = false) => {
  if (initialised) {
    ReactGA.exception({ description, fatal });
  }
};
