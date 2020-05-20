import Link from '@frontend/components/Link';
import React from 'react';
import classNames from 'classnames';
import useMounted from '@common/hooks/useMounted';
import ButtonText from '@common/components/ButtonText';
import { useCookies } from '@frontend/hooks/useCookies';
import styles from './CookieBanner.module.scss';

interface Props {
  wide?: boolean;
}

function CookieBanner({ wide }: Props) {
  const { getCookie, setBannerSeenCookie, setGADisabledCookie } = useCookies();

  const { isMounted } = useMounted(() => {
    if (getCookie('disableGA') === 'true') {
      setGADisabledCookie(true);
    }
  });

  return isMounted && getCookie('bannerSeen') !== 'true' ? (
    <div className={styles.container}>
      <div
        className={classNames('govuk-width-container', 'dfe-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
        <p>
          <span>GOV.UK uses cookies to make the site simpler.</span>{' '}
          <ButtonText
            onClick={() => {
              setBannerSeenCookie(true);
              if (typeof getCookie('disableGA') === 'undefined') {
                setGADisabledCookie(false);
              }
            }}
          >
            Accept Cookies
          </ButtonText>{' '}
          or{' '}
          <Link to="/cookies">
            find out more about cookies and cookie settings
          </Link>
          .
        </p>
      </div>
    </div>
  ) : null;
}

export default CookieBanner;
