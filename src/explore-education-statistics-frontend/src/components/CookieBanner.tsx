import React from 'react';
import useMounted from '@common/hooks/useMounted';
import ButtonText from '@common/components/ButtonText';
import { useCookies } from '@frontend/hooks/useCookies';
import styles from './CookieBanner.module.scss';

function CookieBanner() {
  const { getCookie, setBannerSeenCookie, setGADisabledCookie } = useCookies();
  useMounted(() => {
    if (getCookie('disableGA') === 'true') {
      setGADisabledCookie(true);
    }
  });
  const acceptCookies = () => {
    setBannerSeenCookie(true);
    if (typeof getCookie('disableGA') === 'undefined') {
      setGADisabledCookie(false);
    }
  };

  function render() {
    return (
      <div className={styles.container}>
        <div className="govuk-width-container dfe-width-container--wide">
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
            <a href="/cookies">
              find out more about cookies and cookie settings
            </a>
            .
          </p>
        </div>
      </div>
    );
  }
  return getCookie('bannerSeen') === 'true' ? null : render();
}

export default CookieBanner;
