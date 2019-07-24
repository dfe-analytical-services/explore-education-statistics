import React from 'react';
import { NextContext } from 'next';
import cookie from 'cookie';
import { useCookies } from 'react-cookie';
import ButtonText from '@common/components/ButtonText';
import CookieMap from '@frontend/services/cookieMap';
import useMounted from 'explore-education-statistics-common/src/hooks/useMounted';
import styles from './CookieBanner.module.scss';

interface Props {
  cookies?: any;
}

function CookieBanner({ cookies }: Props) {
  const [liveCookies, setCookie] = useCookies();

  const { isMounted } = useMounted();

  const acceptCookies = () => {
    setCookie(CookieMap.bannerSeenCookie.name, true, {
      expires: CookieMap.bannerSeenCookie.expires,
    });
    if (liveCookies[CookieMap.disableGACookie.name] === undefined) {
      setCookie(CookieMap.disableGACookie.name, false, {
        expires: CookieMap.disableGACookie.expires,
      });
    }
  };

  function render() {
    return (
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
          <a href="/cookies">find out more about cookies and cookie settings</a>
          .
        </p>
      </div>
    );
  }

  if (isMounted) {
    return liveCookies[CookieMap.bannerSeenCookie.name] === 'true'
      ? null
      : render();
  }
  return cookies[CookieMap.bannerSeenCookie.name] === 'true' ? null : render();
}

CookieBanner.getInitialProps = (props: NextContext) => {
  if (props && props.req && props.req.headers && props.req.headers.cookie) {
    return {
      cookies: cookie.parse(props.req.headers.cookie),
    };
  }
  return {
    cookies: {},
  };
};

export default CookieBanner;
