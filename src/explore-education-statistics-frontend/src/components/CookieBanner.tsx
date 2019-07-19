import React from 'react';
import { useCookies } from 'react-cookie';
import ButtonText from '@common/components/ButtonText';
import * as googleAnalyticsService from '@frontend/services/googleAnalyticsService';
import {
  acceptCookies,
  getCookies,
  bannerSeenCookieName,
  cookieSettingsCookieName,
} from '@frontend/services/cookiesService';
import useMounted from 'explore-education-statistics-common/src/hooks/useMounted';
import styles from './CookieBanner.module.scss';

const CookieBanner = () => {
  useMounted(() => {
    if (getCookies()[cookieSettingsCookieName] !== false) {
      googleAnalyticsService.initGA();
    }
  });
  const [cookies, setCookie] = useCookies([bannerSeenCookieName]);

  return cookies[bannerSeenCookieName] !== 'true' ? (
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
