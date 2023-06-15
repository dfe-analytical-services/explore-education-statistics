import PrototypePage from '@admin/prototypes/components/PrototypePage';
import classNames from 'classnames';
import React from 'react';
import styles from './PrototypePublicPage.module.scss';

const PrototypeHomepage = () => {
  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage title="Explore our statistics and data" wide={false}>
        <p className="govuk-body-l">
          Select an option to find the national and regional level statistics
          and data you’re looking for.
        </p>
        <div className={styles.prototypeCardContainer}>
          <div className={styles.prototypeCard}>
            <h2 className="govuk-heading-m govuk-!-margin-bottom-2">
              <a href="#">Find statistics and data</a>
            </h2>
            <p className="govuk-caption-m">
              Browse to find statistical summaries and explanations to help you
              understand and analyse our range of national and regional
              statistics and data.
            </p>
          </div>
          <div className={styles.prototypeCard}>
            <h2 className="govuk-heading-m govuk-!-margin-bottom-2">
              <a href="#">Create your own tables</a>
            </h2>
            <p className="govuk-caption-m">
              Use our online tool to build tables using our range of national
              and regional data.
            </p>
          </div>
        </div>
        <h2 className="govuk-heading-m">Supporting information</h2>
        <div className={styles.prototypeCardContainer}>
          <div
            className={classNames(
              styles.prototypeCard,
              styles.prototypeCardNoBorder,
            )}
          >
            <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
              <a href="#">Education statistics: methodology</a>
            </h3>
            <p className="govuk-caption-m">
              Browse to find out more about the methodology behind education
              statistics and data and how and why they're collected and
              published.
            </p>
          </div>
          <div
            className={classNames(
              styles.prototypeCard,
              styles.prototypeCardNoBorder,
            )}
          >
            <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
              <a href="#">Education statistics: glossary</a>
            </h3>
            <p className="govuk-caption-m">
              Browse our A to Z list of definitions for terms used across
              education statistics and data.
            </p>
          </div>
        </div>
        <h3 className="govuk-heading-m govuk-!-margin-top-9">
          Related services
        </h3>
        <p className="govuk-body">
          Use these services to find specific performance and other information
          about schools and colleges in England:
        </p>
        <div className="govuk-grid-row1 govuk-!-margin-bottom-3">
          <div className="govuk-grid-column-one-half1">
            <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
              <a
                className="govuk-link"
                href="https://www.gov.uk/school-performance-tables"
              >
                Find and compare schools in England
              </a>
            </h4>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              Search for and check the performance of primary, secondary and
              special needs schools and colleges.
            </p>
          </div>
          <div className="govuk-grid-column-one-half1">
            <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
              <a
                className="govuk-link"
                href="https://www.get-information-schools.service.gov.uk/"
              >
                Get information about schools
              </a>
            </h4>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              Search to find and download information about schools, colleges,
              educational organisations and governors in England.
            </p>
          </div>
        </div>
        <div className="govuk-grid-row1 govuk-!-margin-bottom-9">
          <div className="govuk-grid-column-one-half1">
            <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
              <a
                className="govuk-link"
                href="https://schools-financial-benchmarking.service.gov.uk/"
              >
                Schools financial benchmarking
              </a>
            </h4>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              Compare your school's income and expenditure with other schools in
              England.
            </p>
          </div>
          <div className="govuk-grid-column-one-half1">
            <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
              <a
                className="govuk-link"
                href="https://www.gov.uk/government/organisations/department-for-education/about/statistics"
              >
                Statistics at DfE
              </a>
            </h4>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              Browse to find and download statistics and data on education and
              children which are not currently available through explore
              education statistics.
            </p>
          </div>
        </div>
        <hr />
        <h3 className="govuk-heading-l govuk-!-margin-top-9">Contact Us</h3>
        <p className="govuk-body govuk-!-margin-top-1">
          If you need help and support or have a question about education
          statistics and data contact:
        </p>

        <p className="govuk-body govuk-!-margin-top-1">
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
