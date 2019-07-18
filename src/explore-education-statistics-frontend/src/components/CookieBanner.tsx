import React from 'react';
import { useCookies } from 'react-cookie';
import ButtonText from '@common/components/ButtonText';
import * as googleAnalytics from '@frontend/services/googleAnalyticsService';
import useMounted from 'explore-education-statistics-common/src/hooks/useMounted';
import styles from './CookieBanner.module.scss';

export const cookieBannerSeenName = 'dfe_seen_cookie_message';
export function getCookies() {
  // turns document.cookie e.g. 'cookie_name=hello-world; cookietwo=true'
  // into { cookie_name: "hellow-world", cookietwo: true }
  const stringCookies = document.cookie;
  const cookiesPairsArray = stringCookies.split('; ');
  const cookies: any = {};
  cookiesPairsArray.forEach(cookiesPairString => {
    const [key, value]: string[] = cookiesPairString.split('=');
    cookies[key] = value;
  });
  return cookies;
}

const CookieBanner = () => {
  useMounted(() => {
    if (getCookies()[googleAnalytics.cookieEnabled] !== false) {
      googleAnalytics.initGA();
    }
  });
  const [cookies, setCookie] = useCookies([cookieBannerSeenName]);

  const dateToday = new Date();
  const oneMonthFromNow = new Date(
    dateToday.setMonth(dateToday.getMonth() + 1),
  );
  const acceptCookies = () => {
    getCookies();
    setCookie(cookieBannerSeenName, true, {
      expires: oneMonthFromNow,
    });
  };

  return !cookies[cookieBannerSeenName] ? (
    <div className={styles.container}>
      <p>
        <span>GOV.UK uses cookies to make the site simpler.</span>{' '}
        <ButtonText
          type="button"
          className={styles.button}
          onClick={() => {
            acceptCookies();
          }}
        >
          Accept Cookies
        </ButtonText>{' '}
        or{' '}
        <a href="/cookies">find out more about cookies and cookie settings</a>.
      </p>
    </div>
  ) : null;
};

export default CookieBanner;
