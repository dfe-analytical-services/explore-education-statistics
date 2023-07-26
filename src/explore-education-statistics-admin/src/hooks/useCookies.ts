import { Dictionary } from '@common/types';
import { addYears } from 'date-fns';
import { useCookies as useBaseCookies } from 'react-cookie';
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
  adminBannerSeen: Cookie;
}

export const allowedCookies: AllowedCookies = {
  adminBannerSeen: {
    name: 'ees_admin_banner_seen',
    duration: '1 year',
    options: {
      expires: addYears(new Date(), 1),
    },
  },
};

export function useCookies() {
  const [cookies, setBaseCookie] = useBaseCookies();

  const [adminCookies, setAdminCookies] = useState<Dictionary<string>>(cookies);

  const setCookie = (
    name: string,
    value: string,
    options: Cookie['options'],
  ) => {
    setBaseCookie(name, value, {
      secure: true,
      ...options,
    });

    setAdminCookies({
      ...cookies,
      [name]: value,
    });
  };

  return {
    getCookie(cookieKey: keyof AllowedCookies): string {
      if (!allowedCookies[cookieKey]) {
        throw new Error(`Invalid cookie key: '${cookieKey}'`);
      }

      return adminCookies[allowedCookies[cookieKey].name];
    },
    setAdminBannerSeenCookie(isSeen: boolean) {
      const value = isSeen ? 'true' : 'false';

      setCookie(
        allowedCookies.adminBannerSeen.name,
        value,
        allowedCookies.adminBannerSeen.options,
      );
    },
  };
}
