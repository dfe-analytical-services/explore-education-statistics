import React from 'react';
import { useCookies } from '@frontend/hooks/useCookies';
import ButtonText from '@common/components/ButtonText';
import styles from './CookieBanner.module.scss';

function CookieBanner() {
  const { getCookie, setBannerSeenCookie, setGADisabledCookie } = useCookies();

  console.log('±±', getCookie('bannerSeen'));

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
