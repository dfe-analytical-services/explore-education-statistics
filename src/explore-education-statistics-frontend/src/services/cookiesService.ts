import * as googleAnalyticsService from '@frontend/services/googleAnalyticsService';

interface CookieObj {
  name: string;
  value?: any;
  expires?: Date;
}

function oneMonthFromNow() {
  const dateToday = new Date();
  return new Date(dateToday.setMonth(dateToday.getMonth() + 1));
}

export const bannerSeenCookieName = 'ees_banner_seen';
export const cookieSettingsCookieName = 'ees_cookie_settings';

export function getCookies() {
  // turns document.cookie e.g. 'cookie_name=hello-world; cookietwo=true'
  // into { cookie_name: "hellow-world", cookietwo: true }
  const cookies: any = {};
  document.cookie.split('; ').forEach(cookiesPairString => {
    const [key, value]: string[] = cookiesPairString.split('=');
    cookies[key] = value;
  });
  return cookies;
}

export function setCookies(newCookies: CookieObj[]) {
  newCookies.forEach(cookie => {
    const { name, value, expires = oneMonthFromNow() } = cookie;
    document.cookie = `${name}=${JSON.stringify(value)}; expires=${expires}`;

    if (name === cookieSettingsCookieName) {
      const cookieSettingGA = value.googleAnalytics;
      if (cookieSettingGA && cookieSettingGA.length) {
        if (cookieSettingGA === 'off') {
          googleAnalyticsService.disableGA();
        } else {
          googleAnalyticsService.enableGA();
        }
      }
    }
  });
}

export function deleteCookies(cookieNames: string[]) {
  setCookies(
    cookieNames.map(cookieName => {
      return {
        name: cookieName,
        expires: new Date(0),
      };
    }),
  );
}

export function acceptCookies() {
  // Accepted cookies banner
  setCookies([
    {
      name: bannerSeenCookieName,
      value: 'true',
    },
  ]);
}
