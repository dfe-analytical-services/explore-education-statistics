import React from 'react';
// import Details from '../components/Details';
// import PrototypeDataSample from './components/PrototypeDataSample';
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
            for Education (DfE) statistics and data for state-funded schools in
            England on:
          </p>
          <ul className="govuk-bulllet-list">
            <li>children and young people</li>
            <li>further and higher education</li>
            <li>schools</li>
          </ul>
          <p className="govuk-body">The service will let you:</p>
          <ul className="govuk-bulllet-list">
            <li>view annual statistical headlines and trends</li>
            <li>view statistical charts and tables</li>
            <li>create your own charts and tables</li>
            <li>download underlying data files for your own analysis</li>
          </ul>
          <div className="govuk-inset-text">
            <a
              className="govuk-link"
              href="https://www.education-ni.gov.uk/topics/statistics-and-research/statistics"
            >
              Northern Ireland
            </a>
            ,{' '}
            <a
              className="govuk-link"
              href="https://www2.gov.scot/Topics/Statistics/Browse/School-Education"
            >
              Scotland
            </a>
            , and{' '}
            <a
              className="govuk-link"
              href="https://gov.wales/statistics-and-research/?topic=Education+and+skills&lang=en"
            >
              Wales
            </a>{' '}
            have their own websites to help you explore education statistics.
          </div>
          <Link
            to="/prototypes/home"
            className="govuk-button govuk-button--start"
          >
            Start now
          </Link>
          <h2 className="govuk-heading-m govuk-!-margin-top-9">
            Before you start
          </h2>
          <p className="govuk-body">
            The service contains statistics and data going as far back as 2009.
          </p>
          <p className="govuk-body">
            To find and compare and contrast performance and other information
            about schools and colleges near you use:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              <a
                className="govuk-link"
                href="https://www.gov.uk/school-performance-tables"
              >
                Find and compare schools in England
              </a>
            </li>
            <li>
              <a
                className="govuk-link"
                href="https://www.get-information-schools.service.gov.uk"
              >
                Get information about schools
              </a>
            </li>
          </ul>
          <h3 className="govuk-heading-m app-related-items govuk-!-margin-top-9">
            Explore the topic
          </h3>
          <ul className="govuk-list">
            <li>
              <a
                className="govuk-link"
                href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections"
              >
                DfE statistical collections
              </a>
            </li>
            <li>
              <a
                className="govuk-link"
                href="https://www.gov.uk/government/publications?departments%5B%5D=department-for-education&amp;publication_type=transparency-data"
              >
                DfE Transparency data
              </a>
            </li>
            <li>
              <a
                className="govuk-link"
                href="https://www.gov.uk/government/organisations/department-for-education/about/statistics"
              >
                Statistics at DfE
              </a>
            </li>
            <li>
              <a
                className="govuk-link"
                href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education"
              >
                Standards for official statistics published by DfE
              </a>
            </li>
            <li>
              <a
                className="govuk-link"
                href="https://www.statisticsauthority.gov.uk/code-of-practice"
              >
                UK Statistics Authority: Code of Practice for Statistics
              </a>
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
                <a
                  className="govuk-link"
                  href="https://www.gov.uk/topic/schools-colleges-childrens-services/data-collection-statistical-returns"
                >
                  Data collection and statistical returns
                </a>
              </li>
              <li>
                <a
                  className="govuk-link"
                  href="https://www.gov.uk/government/statistics?keywords=&taxons%5B%5D=all&amp;departments%5B%5D=department-for-education&amp;from_date=&amp;to_date="
                >
                  DfE statistics: published
                </a>
              </li>
              <li>
                <a
                  className="govuk-link"
                  href="https://www.gov.uk/government/statistics/announcements?utf8=%E2%9C%93&amp;organisations%5B%5D=department-for-education"
                >
                  DfE statistics: release calendar
                </a>
              </li>
              <li>
                <a
                  className="govuk-link"
                  href="https://www.gov.uk/guidance/how-to-access-department-for-education-dfe-data-extracts"
                >
                  How to access DfE data extracts
                </a>
              </li>
            </ul>
          </aside>
        </div>
      </div>
    </PrototypePage>
  );
};

export default StartPage;
