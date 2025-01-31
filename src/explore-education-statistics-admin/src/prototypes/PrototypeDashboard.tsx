import classNames from 'classnames';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import PrototypeDashboardContent from '@admin/prototypes/components/PrototypeDashboardContent';
import PageSearchForm from '@common/components/PageSearchForm';
import React, { useState } from 'react';
import styles from './PrototypePublicPage.module.scss';

const PrototypeReleaseData = () => {
  const [showContents, setShowContents] = useState(true);

  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage title="Education in numbers" wide={false}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <dl className="govuk-summary-list">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Last updated: </dt>
                <dd className="govuk-summary-list__value">
                  <time>October 2023</time>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Receive updates: </dt>
                <dd className="govuk-summary-list__value">
                  <a
                    data-testid="subscription-children-looked-after-in-england-including-adoptions"
                    href="/subscriptions/new-subscription/children-looked-after-in-england-including-adoptions"
                    className="govuk-link govuk-link--no-visited-state dfe-print-hidden"
                  >
                    Sign up for email alerts
                  </a>
                </dd>
              </div>
            </dl>

            <PageSearchForm inputLabel="Search this page" />
          </div>
        </div>

        <h2 className={styles.contentsMobile}>Contents</h2>

        {!showContents && (
          <>
            <h2 id="contentsNoSideNav">Contents</h2>
            <p>
              <a
                href="#"
                onClick={e => {
                  setShowContents(true);
                  e.preventDefault();
                }}
              >
                Show contents in side panel
              </a>
            </p>
          </>
        )}
        <div className={styles.releaseContainer}>
          <div>
            {showContents && (
              <div className={styles.stickyLinksContainer}>
                <div
                  className={classNames(styles.stickyLinks)}
                  style={{ border: 'none' }}
                >
                  <h2 id="contents" className="govuk-heading-m">
                    Contents
                  </h2>
                  <ul className="govuk-list govuk-list--spaced">
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#children-early-years"
                      >
                        Children and early years
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#schools"
                      >
                        Primary and secondary schools
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#destinations"
                      >
                        Destinations after school
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#children-social-care"
                      >
                        Children's social care
                      </a>
                    </li>
                  </ul>
                </div>
              </div>
            )}
          </div>
          <div className={styles.releaseMainContent}>
            <PrototypeDashboardContent />
          </div>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseData;
