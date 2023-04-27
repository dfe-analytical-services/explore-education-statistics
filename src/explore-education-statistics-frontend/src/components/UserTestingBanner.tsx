import useMounted from '@common/hooks/useMounted';
import { useCookies } from '@frontend/hooks/useCookies';
import ButtonText from '@common/components/ButtonText';
import classNames from 'classnames';
import React from 'react';
import styles from './UserTestingBanner.module.scss';
import Link from './Link';

const UserTestingBanner = () => {
  const { getCookie, setUserTestingBannerSeenCookie } = useCookies();
  const isUserTestingBannerSeen = getCookie('userTestingBannerSeen') === 'true';
  const { isMounted } = useMounted();

  if (isMounted && !isUserTestingBannerSeen) {
    return (
      <div className={styles.container}>
        <div className="govuk-width-container">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-three-quarters">
              <p className={styles.heading}>
                Help develop Explore education statistics
              </p>
              <p>
                <Link
                  className={styles.link}
                  to="https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-XMiKzsnr8xJoWM_DeGwIu9UQVNYVkxZSEJVVjhPOURXSjJVMjhZRTdYMi4u"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Get involved in making this service better
                </Link>
              </p>
            </div>
            <div className="govuk-grid-column-one-quarter dfe-align--right">
              <ButtonText
                className={classNames(styles.link, 'govuk-!-margin-bottom-2')}
                onClick={() => {
                  setUserTestingBannerSeenCookie(true);
                }}
              >
                Close
              </ButtonText>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return null;
};
export default UserTestingBanner;
