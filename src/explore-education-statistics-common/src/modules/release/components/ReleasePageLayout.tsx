import { useMobileMedia } from '@common/hooks/useMedia';
import styles from '@common/modules/release/components/ReleasePageLayout.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

const ReleasePageLayout = ({ children }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={styles.wrapper}>
      {!isMobileMedia && (
        <div className={styles.sidebar}>
          <h2 className="govuk-heading-s" id="nav-label">
            On this page
          </h2>
          <nav aria-labelledby="nav-label" role="navigation">
            <ul className="govuk-list">
              <li>TODO EES-6476 Build nav</li>

              <li className="govuk-!-margin-top-8">
                <a
                  className="govuk-link--no-visited-state govuk-link--no-underline"
                  href="#main-content"
                >
                  Back to top
                </a>
              </li>
            </ul>
          </nav>
        </div>
      )}
      <div className={styles.content}>{children}</div>
    </div>
  );
};

export default ReleasePageLayout;
