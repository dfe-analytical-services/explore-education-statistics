import classNames from 'classnames';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import PrototypeDashboardContent from '@admin/prototypes/components/PrototypeDashboardContent';
import React from 'react';
import styles from './PrototypePublicPage.module.scss';

const PrototypeReleaseData = () => {
  // const [showContents, setShowContents] = useState(true);

  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage wide={false}>
        <div className={styles.releaseContainer}>
          <div>
            <div className={styles.stickyLinksContainer}>
              <h2 id="contentsNoSideNav" className="govuk-heading-m">
                Themes
              </h2>
              <div
                className={classNames(styles.stickyLinks)}
                style={{ border: 'none' }}
              >
                <div
                  className="dfe-flex dfe-justify-content--space-between"
                  style={{
                    flexDirection: 'column',
                  }}
                >
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
                  <hr />
                  <div className="govuk-!-margin-bottom-9">
                    <a href="#">Back to top</a>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className={styles.releaseMainContent}>
            <h1 className="govuk-heading-xl">Education in numbers</h1>
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
                    <dt className="govuk-summary-list__key">
                      Receive updates:{' '}
                    </dt>
                    <dd className="govuk-summary-list__value">
                      <a
                        data-testid="subscription-children-looked-after-in-england-including-adoptions"
                        href="/subscriptions?slug=children-looked-after-in-england-including-adoptions"
                        className="govuk-link govuk-link--no-visited-state dfe-print-hidden"
                      >
                        Sign up for email alerts
                      </a>
                    </dd>
                  </div>
                </dl>
              </div>
            </div>
            <p className="govuk-!-margin-bottom-9">
              This page gives a high level overview of education statistics in
              England, with links to specific publications that give more detail
              on the topic areas. The figures provided are the latest available
              and may relate to different time periods, please see individual
              publications for methodological detail.
            </p>
            <PrototypeDashboardContent />
          </div>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseData;
