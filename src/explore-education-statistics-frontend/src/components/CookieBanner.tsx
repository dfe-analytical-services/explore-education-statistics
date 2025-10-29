import Link from '@frontend/components/Link';
import { PageWidth } from '@frontend/components/Page';
import React, { useEffect, useState } from 'react';
import useMounted from '@common/hooks/useMounted';
import { useCookies } from '@frontend/hooks/useCookies';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import classNames from 'classnames';
import styles from './CookieBanner.module.scss';

interface Props {
  width?: PageWidth;
}

function CookieBanner({ width }: Props) {
  const { getCookie, setBannerSeenCookie, setGADisabledCookie } = useCookies();
  const isBannerSeen = getCookie('bannerSeen') === 'true';
  const [isVisible, setVisible] = useState(!isBannerSeen);
  const { isMounted } = useMounted();

  useEffect(() => {
    setVisible(!isBannerSeen);
  }, [isBannerSeen]);

  return isMounted && isVisible ? (
    <div
      className={classNames('govuk-cookie-banner', styles.container)}
      data-nosnippet
      role="region"
      aria-label="Cookies on Explore Education Statistics"
    >
      {typeof getCookie('disableGA') === 'undefined' ? (
        <div
          className={classNames(
            'govuk-cookie-banner__message govuk-width-container',
            {
              'dfe-width-container--wide': width === 'wide',
              'dfe-width-container--full': width === 'full',
            },
          )}
        >
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <h2 className="govuk-cookie-banner__heading govuk-heading-m">
                Cookies on Explore Education Statistics
              </h2>

              <div className="govuk-cookie-banner__content">
                <p>We use some essential cookies to make this service work.</p>
                <p>
                  We’d also like to use analytics cookies so we can understand
                  how you use the service and make improvements.
                </p>
              </div>
            </div>
          </div>

          <ButtonGroup verticalSpacing="m" className="govuk-!-margin-bottom-4">
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
          className={classNames(
            'govuk-cookie-banner__message govuk-width-container',
            {
              'dfe-width-container--wide': width === 'wide',
              'dfe-width-container--full': width === 'full',
            },
          )}
          role="alert"
        >
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <div className="govuk-cookie-banner__content">
                <p>
                  {`You’ve ${
                    getCookie('disableGA') === 'false' ? 'accepted' : 'rejected'
                  } analytics cookies. `}
                  You can <Link to="/cookies">change your cookie settings</Link>{' '}
                  at any time.
                </p>
              </div>
            </div>
          </div>

          <ButtonGroup verticalSpacing="m" className="govuk-!-margin-bottom-4">
            <Button
              onClick={() => {
                setBannerSeenCookie(true);
                setVisible(false);
              }}
            >
              Hide cookie message
            </Button>
          </ButtonGroup>
        </div>
      )}
    </div>
  ) : null;
}

export default CookieBanner;
