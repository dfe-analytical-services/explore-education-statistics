import useMounted from '@common/hooks/useMounted';
import { Dictionary } from '@common/types';
import {
  disableGoogleAnalytics,
  enableGoogleAnalytics,
  googleAnalyticsCookies,
} from '@frontend/services/googleAnalyticsService';
import { addMonths, addYears } from 'date-fns';
import {
  destroyCookie,
  parseCookies,
  setCookie as setBaseCookie,
} from 'nookies';
import { useState } from 'react';

interface Cookie {
  name: string;
  duration?: string;
  options: {
    expires: Date;
    secure?: boolean;
  };
}

interface AllowedCookies {
  bannerSeen: Cookie;
  disableGA: Cookie;
}

export const allowedCookies: AllowedCookies = {
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

export function useCookies(initialCookies?: Dictionary<string>) {
  const [cookies, setCookies] = useState<Dictionary<string>>(
    initialCookies ?? {},
  );

  useMounted(() => {
    if (!initialCookies) {
      setCookies(parseCookies());
    }
  });

  const setCookie = (
    name: string,
    value: string,
    options: Cookie['options'],
  ) => {
    setBaseCookie(null, name, value, options);

    setCookies({
      ...cookies,
      [name]: value,
    });
  };

  return {
    getCookie(cookieKey: keyof AllowedCookies): string {
      if (!allowedCookies[cookieKey]) {
        throw new Error(`Invalid cookie key: '${cookieKey}'`);
      }

      return cookies[allowedCookies[cookieKey].name];
    },
    setBannerSeenCookie(isSeen: boolean) {
      const value = isSeen ? 'true' : 'false';

      setCookie(
        allowedCookies.bannerSeen.name,
        value,
        allowedCookies.bannerSeen.options,
      );
    },
    setGADisabledCookie(isDisabled: boolean) {
      const value = isDisabled ? 'true' : 'false';

      setCookie(
        allowedCookies.disableGA.name,
        value,
        allowedCookies.disableGA.options,
      );

      if (isDisabled) {
        disableGoogleAnalytics();
        googleAnalyticsCookies.forEach(cookieName =>
          destroyCookie(null, cookieName),
        );
      } else {
        enableGoogleAnalytics();
      }
    },
  };
}
