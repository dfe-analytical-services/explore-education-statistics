import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';
import classNames from 'classnames';
import Link from '../components/Link';
import styles from './PrototypePublicPage.module.scss';

const PrototypeHomepage = () => {
  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage title="Explore our statistics and data" wide={false}>
        <div
          className={classNames(
            styles.prototypeCardContainer,
            styles.prototypeCardBg,
          )}
        >
          <div className={classNames(styles.prototypeCardChevron)}>
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
            <p className="govuk-body govuk-!-margin-bottom-0">
              Browse statistical summaries and download associated data to help
              you understand and analyse our range of statistics.
            </p>
          </div>
          <div className={classNames(styles.prototypeCardChevron)}>
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
            <p className="govuk-body govuk-!-margin-bottom-0">
              Explore our range of data and build your own tables from it.
            </p>
          </div>
        </div>
        <h2 className="govuk-heading-l">Supporting information</h2>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
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
              Browse to find out more about the methodology behind our
              statistics and how and why they&apos;re collected and published.
            </p>

            <h3 className="govuk-!-margin-bottom-1">
              <Link to="/glossary">Glossary</Link>
            </h3>
            <p className="govuk-caption-m">
              Browse our A to Z list of definitions for terms used across our
              statistics.
            </p>
          </div>
        </div>
        <hr />
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
          The Explore education statistics service is operated by the Department
          for Education (DfE).
        </p>

        <p className="govuk-!-margin-top-1">
          If you need help and support or have a question about Explore
          education statistics contact:
        </p>

        <p className="govuk-!-margin-top-1">
          <strong>Explore education statistics team</strong>
        </p>

        <p className="govuk-caption-m govuk-!-margin-top-1">
          Email
          <br />
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>
        </p>
      </PrototypePage>
    </div>
  );
};

export default PrototypeHomepage;
