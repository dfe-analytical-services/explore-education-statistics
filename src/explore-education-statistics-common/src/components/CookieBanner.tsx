import React from 'react';
import styles from './CookieBanner.module.scss';

const CookieBanner = () => {
  return (
    <div className={styles.container}>
      <p>
        <span>GOV.UK uses cookies to make the site simpler.</span>{' '}
        <button type="button" className={styles.button}>
          Accept Cookies
        </button>{' '}
        or <a href="/cookie-policy">find out more about cookies</a>.
      </p>
    </div>
  );
};

export default CookieBanner;
