import Link from '@frontend/components/Link';
import React, { useEffect, useState } from 'react';
import useMounted from '@common/hooks/useMounted';
import { useCookies } from '@frontend/hooks/useCookies';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import classNames from 'classnames';
import styles from './CookieBanner.module.scss';

interface Props {
  wide?: boolean;
}

function CookieBanner({ wide }: Props) {
  const { getCookie, setBannerSeenCookie, setGADisabledCookie } = useCookies();
  const isBannerSeen = getCookie('bannerSeen') === 'true';
  const [isVisible, setVisible] = useState(!isBannerSeen);
  const { isMounted } = useMounted();

  useEffect(() => {
    setVisible(!isBannerSeen);
  }, [isBannerSeen]);

  return isMounted && isVisible ? (
    <div className={styles.container}>
      <div
        className={classNames('govuk-cookie-banner', 'govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
        role="region"
        aria-label="Cookies on Explore Education Statistics"
      >
        {typeof getCookie('disableGA') === 'undefined' ? (
          <div className="govuk-cookie-banner__message govuk-width-container">
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                <h2 className="govuk-cookie-banner__heading govuk-heading-m">
                  Cookies on Explore Education Statistics
                </h2>

                <div className="govuk-cookie-banner__content">
                  <p>
                    We use some essential cookies to make this service work.
                  </p>
                  <p>
                    We’d also like to use analytics cookies so we can understand
                    how you use the service and make improvements.
                  </p>
                </div>
              </div>
            </div>

            <ButtonGroup>
              <Button
                onClick={() => {
                  setBannerSeenCookie(true);
                  setGADisabledCookie(false);
                }}
              >
                Accept analytics cookies
              </Button>{' '}
              <Button
                onClick={() => {
                  setBannerSeenCookie(true);
                  setGADisabledCookie(true);
                }}
              >
                Reject analytics cookies
              </Button>{' '}
              <Link to="/cookies">View cookies</Link>
            </ButtonGroup>
          </div>
        ) : (
          <div
            className="govuk-cookie-banner__message govuk-width-container"
            role="alert"
          >
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                <div className="govuk-cookie-banner__content">
                  <p>
                    {`You’ve ${
                      getCookie('disableGA') === 'false'
                        ? 'accepted'
                        : 'rejected'
                    } analytics cookies. `}
                    You can{' '}
                    <Link to="/cookies">change your cookie settings</Link> at
                    any time.
                  </p>
                </div>
              </div>
            </div>

            <ButtonGroup>
              <Button
                onClick={() => {
                  setBannerSeenCookie(true);
                  setVisible(false);
                }}
              >
                Hide this message
              </Button>
            </ButtonGroup>
          </div>
        )}
      </div>
    </div>
  ) : null;
}

export default CookieBanner;
