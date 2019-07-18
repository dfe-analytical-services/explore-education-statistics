import React from 'react';
import { useCookies } from 'react-cookie';
import ButtonText from '@common/components/ButtonText';
import styles from './CookieBanner.module.scss';

export const cookieBannerSeenName = 'dfe_seen_cookie_message';

const CookieBanner = () => {
  const [cookies, setCookie] = useCookies([cookieBannerSeenName]);

  const dateToday = new Date();
  const oneMonthFromNow = new Date(
    dateToday.setMonth(dateToday.getMonth() + 1),
  );
  const acceptCookies = () => {
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
