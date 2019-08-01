import { useCookies as useBaseCookies } from 'react-cookie';
import {
  enableGA,
  disableGA,
  googleAnalyticsCookies,
} from '@frontend/services/googleAnalyticsService';
import { addMonths, addYears } from 'date-fns';

interface Cookie {
  name: string;
  expires: Date;
  duration?: string;
}

interface CookieMap {
  bannerSeen: Cookie;
  disableGA: Cookie;
}

export const cookieMap: CookieMap = {
  bannerSeen: {
    name: 'ees_banner_seen',
    expires: addMonths(new Date(), 1),
    duration: '1 month',
  },
  disableGA: {
    name: 'ees_disable_google_analytics',
    expires: addYears(new Date(), 10),
    duration: '10 years',
  },
};

export function useCookies() {
  const [liveCookies, setCookie, removeCookie] = useBaseCookies();

  return {
    getCookie(cookieKey: keyof CookieMap) {
      return liveCookies[cookieMap[cookieKey].name];
    },
    setBannerSeenCookie(isSeen: boolean) {
      setCookie(cookieMap.bannerSeen.name, isSeen, {
        expires: cookieMap.bannerSeen.expires,
      });
    },
    setGADisabledCookie(isDisabled: boolean) {
      setCookie(cookieMap.disableGA.name, isDisabled, {
        expires: cookieMap.disableGA.expires,
      });
      if (isDisabled) {
        disableGA();
        googleAnalyticsCookies.forEach(cookieName => removeCookie(cookieName));
      } else {
        enableGA();
      }
    },
  };
}
