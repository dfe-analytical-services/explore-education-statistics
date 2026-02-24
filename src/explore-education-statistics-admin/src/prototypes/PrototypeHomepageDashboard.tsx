// import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';
import classNames from 'classnames';
import PageFooter from '@admin/components/PageFooter';
import PrototypeDashboardContent from '@admin/prototypes/components/PrototypeDashboardContent';
import Tag from '@common/components/Tag';
import Link from '../components/Link';
import styles from './PrototypePublicPage.module.scss';

const PrototypeHomepage = () => {
  return (
    <>
      <title>Explore education statistics - GOV.UK</title>
      <a href="#main-content" className="govuk-skip-link">
        Skip to main content
      </a>

      <header className="govuk-header " role="banner" data-module="header">
        <div className="govuk-header__container">
          <div className="govuk-width-container">
            <div className="govuk-header__logo">
              <a
                href="//www.gov.uk"
                className="govuk-header__link govuk-header__link--homepage"
              >
                <svg
                  focusable="false"
                  role="img"
                  className="govuk-header__logotype"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 148 30"
                  height="30"
                  width="148"
                  aria-label="GOV.UK"
                >
                  <title>GOV.UK</title>
                  <path d="M22.6 10.4c-1 .4-2-.1-2.4-1-.4-.9.1-2 1-2.4.9-.4 2 .1 2.4 1s-.1 2-1 2.4m-5.9 6.7c-.9.4-2-.1-2.4-1-.4-.9.1-2 1-2.4.9-.4 2 .1 2.4 1s-.1 2-1 2.4m10.8-3.7c-1 .4-2-.1-2.4-1-.4-.9.1-2 1-2.4.9-.4 2 .1 2.4 1s0 2-1 2.4m3.3 4.8c-1 .4-2-.1-2.4-1-.4-.9.1-2 1-2.4.9-.4 2 .1 2.4 1s-.1 2-1 2.4M17 4.7l2.3 1.2V2.5l-2.3.7-.2-.2.9-3h-3.4l.9 3-.2.2c-.1.1-2.3-.7-2.3-.7v3.4L15 4.7c.1.1.1.2.2.2l-1.3 4c-.1.2-.1.4-.1.6 0 1.1.8 2 1.9 2.2h.7c1-.2 1.9-1.1 1.9-2.1 0-.2 0-.4-.1-.6l-1.3-4c-.1-.2 0-.2.1-.3m-7.6 5.7c.9.4 2-.1 2.4-1 .4-.9-.1-2-1-2.4-.9-.4-2 .1-2.4 1s0 2 1 2.4m-5 3c.9.4 2-.1 2.4-1 .4-.9-.1-2-1-2.4-.9-.4-2 .1-2.4 1s.1 2 1 2.4m-3.2 4.8c.9.4 2-.1 2.4-1 .4-.9-.1-2-1-2.4-.9-.4-2 .1-2.4 1s0 2 1 2.4m14.8 11c4.4 0 8.6.3 12.3.8 1.1-4.5 2.4-7 3.7-8.8l-2.5-.9c.2 1.3.3 1.9 0 2.7-.4-.4-.8-1.1-1.1-2.3l-1.2 4c.7-.5 1.3-.8 2-.9-1.1 2.5-2.6 3.1-3.5 3-1.1-.2-1.7-1.2-1.5-2.1.3-1.2 1.5-1.5 2.1-.1 1.1-2.3-.8-3-2-2.3 1.9-1.9 2.1-3.5.6-5.6-2.1 1.6-2.1 3.2-1.2 5.5-1.2-1.4-3.2-.6-2.5 1.6.9-1.4 2.1-.5 1.9.8-.2 1.1-1.7 2.1-3.5 1.9-2.7-.2-2.9-2.1-2.9-3.6.7-.1 1.9.5 2.9 1.9l.4-4.3c-1.1 1.1-2.1 1.4-3.2 1.4.4-1.2 2.1-3 2.1-3h-5.4s1.7 1.9 2.1 3c-1.1 0-2.1-.2-3.2-1.4l.4 4.3c1-1.4 2.2-2 2.9-1.9-.1 1.5-.2 3.4-2.9 3.6-1.9.2-3.4-.8-3.5-1.9-.2-1.3 1-2.2 1.9-.8.7-2.3-1.2-3-2.5-1.6.9-2.2.9-3.9-1.2-5.5-1.5 2-1.3 3.7.6 5.6-1.2-.7-3.1 0-2 2.3.6-1.4 1.8-1.1 2.1.1.2.9-.3 1.9-1.5 2.1-.9.2-2.4-.5-3.5-3 .6 0 1.2.3 2 .9l-1.2-4c-.3 1.1-.7 1.9-1.1 2.3-.3-.8-.2-1.4 0-2.7l-2.9.9C1.3 23 2.6 25.5 3.7 30c3.7-.5 7.9-.8 12.3-.8m28.3-11.6c0 .9.1 1.7.3 2.5.2.8.6 1.5 1 2.2.5.6 1 1.1 1.7 1.5.7.4 1.5.6 2.5.6.9 0 1.7-.1 2.3-.4s1.1-.7 1.5-1.1c.4-.4.6-.9.8-1.5.1-.5.2-1 .2-1.5v-.2h-5.3v-3.2h9.4V28H55v-2.5c-.3.4-.6.8-1 1.1-.4.3-.8.6-1.3.9-.5.2-1 .4-1.6.6s-1.2.2-1.8.2c-1.5 0-2.9-.3-4-.8-1.2-.6-2.2-1.3-3-2.3-.8-1-1.4-2.1-1.8-3.4-.3-1.4-.5-2.8-.5-4.3s.2-2.9.7-4.2c.5-1.3 1.1-2.4 2-3.4.9-1 1.9-1.7 3.1-2.3 1.2-.6 2.6-.8 4.1-.8 1 0 1.9.1 2.8.3.9.2 1.7.6 2.4 1s1.4.9 1.9 1.5c.6.6 1 1.3 1.4 2l-3.7 2.1c-.2-.4-.5-.9-.8-1.2-.3-.4-.6-.7-1-1-.4-.3-.8-.5-1.3-.7-.5-.2-1.1-.2-1.7-.2-1 0-1.8.2-2.5.6-.7.4-1.3.9-1.7 1.5-.5.6-.8 1.4-1 2.2-.3.8-.4 1.9-.4 2.7zM71.5 6.8c1.5 0 2.9.3 4.2.8 1.2.6 2.3 1.3 3.1 2.3.9 1 1.5 2.1 2 3.4s.7 2.7.7 4.2-.2 2.9-.7 4.2c-.4 1.3-1.1 2.4-2 3.4-.9 1-1.9 1.7-3.1 2.3-1.2.6-2.6.8-4.2.8s-2.9-.3-4.2-.8c-1.2-.6-2.3-1.3-3.1-2.3-.9-1-1.5-2.1-2-3.4-.4-1.3-.7-2.7-.7-4.2s.2-2.9.7-4.2c.4-1.3 1.1-2.4 2-3.4.9-1 1.9-1.7 3.1-2.3 1.2-.5 2.6-.8 4.2-.8zm0 17.6c.9 0 1.7-.2 2.4-.5s1.3-.8 1.7-1.4c.5-.6.8-1.3 1.1-2.2.2-.8.4-1.7.4-2.7v-.1c0-1-.1-1.9-.4-2.7-.2-.8-.6-1.6-1.1-2.2-.5-.6-1.1-1.1-1.7-1.4-.7-.3-1.5-.5-2.4-.5s-1.7.2-2.4.5-1.3.8-1.7 1.4c-.5.6-.8 1.3-1.1 2.2-.2.8-.4 1.7-.4 2.7v.1c0 1 .1 1.9.4 2.7.2.8.6 1.6 1.1 2.2.5.6 1.1 1.1 1.7 1.4.6.3 1.4.5 2.4.5zM88.9 28 83 7h4.7l4 15.7h.1l4-15.7h4.7l-5.9 21h-5.7zm28.8-3.6c.6 0 1.2-.1 1.7-.3.5-.2 1-.4 1.4-.8.4-.4.7-.8.9-1.4.2-.6.3-1.2.3-2v-13h4.1v13.6c0 1.2-.2 2.2-.6 3.1s-1 1.7-1.8 2.4c-.7.7-1.6 1.2-2.7 1.5-1 .4-2.2.5-3.4.5-1.2 0-2.4-.2-3.4-.5-1-.4-1.9-.9-2.7-1.5-.8-.7-1.3-1.5-1.8-2.4-.4-.9-.6-2-.6-3.1V6.9h4.2v13c0 .8.1 1.4.3 2 .2.6.5 1 .9 1.4.4.4.8.6 1.4.8.6.2 1.1.3 1.8.3zm13-17.4h4.2v9.1l7.4-9.1h5.2l-7.2 8.4L148 28h-4.9l-5.5-9.4-2.7 3V28h-4.2V7zm-27.6 16.1c-1.5 0-2.7 1.2-2.7 2.7s1.2 2.7 2.7 2.7 2.7-1.2 2.7-2.7-1.2-2.7-2.7-2.7z" />
                </svg>
              </a>
            </div>
            <div className="govuk-header__content">
              <a
                href="/"
                className="govuk-header__link govuk-header__link--service-name"
              >
                Explore education statistics
              </a>
            </div>
          </div>
        </div>
      </header>
      <div className={styles.prototypeMasthead}>
        <div className="govuk-width-container">
          <div className="govuk-phase-banner">
            <p className="govuk-phase-banner__content">
              <Tag className="govuk-phase-banner__content__tag">Prototype</Tag>

              <span className="govuk-phase-banner__text">
                This is a prototype page –{' '}
                <Link to="/prototypes">View prototype index</Link>
              </span>
            </p>
          </div>
          <h1 className="govuk-heading-xl  govuk-!-margin-bottom-7">
            Welcome to explore education statistics
          </h1>
          <p className="govuk-body-l govuk-!-margin-bottom-3">
            Explore and download up to date official educational statistics from
            England.
          </p>
          <p className="govuk-body-l">
            <strong>Publications, data downloads, table creation.</strong>
          </p>
        </div>
      </div>
      <div className={styles.prototypeMastheadLower}>
        <div className={classNames('govuk-width-container')}>
          <div
            className={classNames(
              styles.prototypeCardContainerGrid,
              styles.col3,
              styles.prototypeCardBg,
            )}
          >
            <div
              className={classNames(styles.prototypeCardChevron)}
              style={{ border: 'none' }}
            >
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/find-statistics6"
                  className={classNames(
                    styles.prototypeCardChevronLink,
                    'govuk-link--no-visited-state',
                  )}
                >
                  Find statistics and data
                </a>
              </h2>
              <p className="govuk-body-l govuk-!-margin-bottom-0">
                Browse statistical summaries and download associated data to
                help you understand and analyse our range of statistics.
              </p>
            </div>
            <div
              className={classNames(styles.prototypeCardChevron)}
              style={{ border: 'none' }}
            >
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/table-tool"
                  className={classNames(
                    styles.prototypeCardChevronLink,
                    'govuk-link--no-visited-state',
                  )}
                >
                  Create your own tables
                </a>
              </h2>
              <p className="govuk-body-l govuk-!-margin-bottom-0">
                Explore our range of data and build your own tables from it.
              </p>
            </div>
            <div
              className={classNames(styles.prototypeCardChevron)}
              style={{ border: 'none' }}
            >
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/dashboard2"
                  className={classNames(
                    styles.prototypeCardChevronLink,
                    'govuk-link--no-visited-state',
                  )}
                >
                  Education in numbers
                </a>
              </h2>
              <p className="govuk-body-l govuk-!-margin-bottom-0">
                View high level statistics across our range of publications
              </p>
            </div>
          </div>
        </div>
      </div>
      <div className={classNames('govuk-width-container')}>
        <div className={styles.prototypePublicPage}>
          <h2 className="govuk-!-margin-top-9">Featured headlines</h2>

          <PrototypeDashboardContent headlines />

          <h2 className="govuk-!-margin-top-9">Supporting information</h2>

          <h3 className="govuk-!-margin-bottom-1">
            <Link to="/data-catalogue">Data catalogue</Link>
          </h3>
          <p className="govuk-caption-m">
            View all of the open data available and choose files to download.
          </p>

          <h3 className="govuk-!-margin-bottom-1">
            <Link to="/methodology">Methodology</Link>
          </h3>
          <p className="govuk-caption-m">
            Browse to find out more about the methodology behind our statistics
            and how and why they&apos;re collected and published.
          </p>

          <h3 className="govuk-!-margin-bottom-1">
            <Link to="/glossary">Glossary</Link>
          </h3>
          <p className="govuk-caption-m">
            Browse our A to Z list of definitions for terms used across our
            statistics.
          </p>

          <h2 className="govuk-!-margin-top-9">Related services</h2>

          <div className="govuk-grid-row govuk-!-margin-bottom-3">
            <div className="govuk-grid-column-two-thirds">
              <p>
                Use these services to find related information and other
                statistical services provided by the Department for Education
                (DfE):
              </p>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics">
                  Statistics at DfE
                </a>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Find out more about latest news, announcements, forthcoming
                releases and ad hoc publications, as well as related education
                statistics.
              </p>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <a href="https://www.gov.uk/school-performance-tables">
                  Compare school and college performance
                </a>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Search for and check the performance of primary, secondary and
                special needs schools and colleges.
              </p>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <a href="https://www.get-information-schools.service.gov.uk/">
                  Get information about schools
                </a>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Search to find and download information about schools, colleges,
                educational organisations and governors in England.
              </p>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <a href="https://schools-financial-benchmarking.service.gov.uk/">
                  Schools financial benchmarking
                </a>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Compare your school&apos;s income and expenditure with other
                schools in England.
              </p>
            </div>
          </div>

          <hr />

          <h2 className="govuk-!-margin-top-9">Contact us</h2>

          <p className="govuk-!-margin-top-1">
            The Explore education statistics service is operated by the
            Department for Education (DfE).
          </p>

          <p className="govuk-!-margin-top-1">
            If you need help and support or have a question about Explore
            education statistics contact:
          </p>

          <p className="govuk-!-margin-top-1">
            <strong>Explore education statistics team</strong>
          </p>

          <p className="govuk-caption-m govuk-!-margin-top-1 govuk-!-margin-bottom-9">
            Email
            <br />
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
          </p>
        </div>
      </div>
      <PageFooter />
    </>
  );
};

export default PrototypeHomepage;
