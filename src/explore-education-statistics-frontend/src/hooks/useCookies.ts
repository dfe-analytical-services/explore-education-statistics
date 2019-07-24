import { useCookies as useBaseCookies } from 'react-cookie';
import {
  enableGA,
  disableGA,
  googleAnalyticsCookies,
} from '@frontend/services/googleAnalyticsService';

function daysFromNow(days = 0) {
  const result = new Date();
  if (!days) {
    // Add one month
    result.setMonth([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 0][result.getMonth()]);
  } else {
    result.setDate(result.getDate() + days);
  }
  return result;
}

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
    expires: daysFromNow(),
    duration: '1 month',
  },
  disableGA: {
    name: 'ees_disable_google_analytics',
    expires: daysFromNow(),
    duration: '1 month',
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
    setGACookie(isDisabled: boolean) {
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
