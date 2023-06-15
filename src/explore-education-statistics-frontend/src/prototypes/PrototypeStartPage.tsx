import React from 'react';
import Link from '../components/Link';
import PrototypePage from './components/PrototypePage';

const StartPage = () => {
  return (
    <PrototypePage breadcrumbs={[]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">Explore education statistics</h1>
          <p className="govuk-body">
            Use this service to find, download and explore official Department
            for Education (DfE) statistics and data in England for:
          </p>
          <ul className="govuk-bulllet-list">
            <li>children and young people - including social care</li>
            <li>further and higher education</li>
            <li>
              schools - including special schools and pupil referral units
            </li>
          </ul>
          <p className="govuk-body">The service will let you:</p>
          <ul className="govuk-bulllet-list">
            <li>
              view current and historical statistics - including annual
              headlines and trends
            </li>
            <li>view statistical charts and tables</li>
            <li>create your own tables</li>
            <li>download data files for your own analysis</li>
          </ul>
          <div className="govuk-inset-text">
            <Link
              className="govuk-link"
              to="https://www.education-ni.gov.uk/topics/statistics-and-research/statistics"
            >
              Northern Ireland
            </Link>
            ,{' '}
            <Link
              className="govuk-link"
              to="https://www2.gov.scot/Topics/Statistics/Browse/School-Education"
            >
              Scotland
            </Link>
            , and{' '}
            <Link
              className="govuk-link"
              to="https://gov.wales/statistics-and-research/?topic=Education+and+skills&lang=en"
            >
              Wales
            </Link>{' '}
            have their own websites to help you explore education statistics.
          </div>
          <Link to="/" className="govuk-button govuk-button--start">
            Start now
          </Link>
          <h2 className="govuk-heading-m govuk-!-margin-top-9">
            Before you start
          </h2>
          <p className="govuk-body">
            To find specific performance and other information about schools and
            colleges near you use:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="https://www.gov.uk/school-performance-tables">
                Find and compare schools in England
              </Link>
            </li>
            <li>
              <Link to="https://www.get-information-schools.service.gov.uk">
                Get information about schools
              </Link>
            </li>
          </ul>
        </div>
        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items">
            <h2 className="govuk-heading-m" id="subsection-title">
              Related content
            </h2>
            <ul className="govuk-list">
              <li>
                <Link to="https://www.gov.uk/government/organisations/department-for-education/about/statistics">
                  Statistics at DfE
                </Link>
              </li>
              <li>
                <Link to="https://www.gov.uk/government/statistics/announcements?utf8=%E2%9C%93&amp;organisations%5B%5D=department-for-education">
                  DfE statistics: release calendar
                </Link>
              </li>
              <li>
                <Link to="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education">
                  Standards for official statistics published by DfE
                </Link>
              </li>
              <li>
                <Link to="https://www.statisticsauthority.gov.uk/code-of-practice">
                  UK Statistics Authority: Code of Practice for Statistics
                </Link>
              </li>
            </ul>
          </aside>
        </div>
      </div>
    </PrototypePage>
  );
};

export default StartPage;
