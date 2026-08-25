import Link from '@frontend/components/Link';
import styles from '@frontend/modules/home/components/HomePageMasthead.module.scss';
import HomeSearchForm from '@frontend/modules/home/components/HomeSearchForm';
// import SearchDataSearchForm from '@frontend/modules/search-data/components/SearchDataSearchForm';
import React from 'react';

const HomePageMasthead = () => (
  <div className={styles.masthead}>
    <div className="govuk-width-container dfe-width-container--wide govuk-!-padding-top-6 govuk-!-padding-bottom-9">
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1>Explore Education Statistics</h1>
          <p>
            View statistical releases, find data sets and create tables from the
            Department for Education
          </p>

          <HomeSearchForm onSubmit={() => {}} />
        </div>
        <div className="govuk-grid-column-one-third">
          <nav role="navigation" aria-labelledby="quick-links">
            <h2
              className="govuk-heading-m govuk-!-margin-top-9"
              id="quick-links"
            >
              Quick links
            </h2>
            <ul className="govuk-list">
              <li>
                <Link to="/find-statistics" className={styles.link}>
                  Explore publications
                </Link>
              </li>
              <li>
                <Link to="/data-catalogue" className={styles.link}>
                  Browse data catalogue
                </Link>
              </li>
              <li>
                <Link to="/data-tables" className={styles.link}>
                  Create your own tables
                </Link>
              </li>
              <li>
                <Link to="/glossary" className={styles.link}>
                  Glossary
                </Link>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  </div>
);

export default HomePageMasthead;
