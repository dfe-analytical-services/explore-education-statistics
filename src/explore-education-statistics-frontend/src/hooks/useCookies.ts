import { useCookies as useBaseCookies } from 'react-cookie';
import {
  enableGA,
  disableGA,
  googleAnalyticsCookies,
} from '@frontend/services/googleAnalyticsService';
import { addMonths, addYears } from 'date-fns';

interface Cookie {
  name: string;
  duration?: string;
  options: {
    expires: Date;
    secure?: boolean;
    [key: string]: any;
  };
}

interface CookieMap {
  bannerSeen: Cookie;
  disableGA: Cookie;
}

export const cookieMap: CookieMap = {
  bannerSeen: {
    name: 'ees_banner_seen',
    duration: '1 month',
    options: {
      expires: addMonths(new Date(), 1),
      secure: true,
    },
  },
  disableGA: {
    name: 'ees_disable_google_analytics',
    duration: '10 years',
    options: {
      expires: addYears(new Date(), 10),
      secure: true,
    },
  },
};

export function useCookies() {
  const [liveCookies, setCookie, removeCookie] = useBaseCookies();

  return {
    getCookie(cookieKey: keyof CookieMap) {
      return liveCookies[cookieMap[cookieKey].name];
    },
    setBannerSeenCookie(isSeen: boolean) {
      setCookie(
        cookieMap.bannerSeen.name,
        isSeen,
        cookieMap.bannerSeen.options,
      );
    },
    setGADisabledCookie(isDisabled: boolean) {
      setCookie(
        cookieMap.disableGA.name,
        isDisabled,
        cookieMap.disableGA.options,
      );
      if (isDisabled) {
        disableGA();
        googleAnalyticsCookies.forEach(cookieName => removeCookie(cookieName));
      } else {
        enableGA();
      }
    },
  };
}
