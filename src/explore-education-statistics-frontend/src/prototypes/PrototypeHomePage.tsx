import React from 'react';
// import Details from '../components/Details';
import Link from '../components/Link';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';
// import PrototypeTileWithChart from './components/PrototypeTileWithChart';

const HomePage = () => {
  return (
    <PrototypePage>
      <h1 className="govuk-heading-xl">
        Choose how to explore our statistics and data
      </h1>
      <p className="govuk-body-l">
        Select an option to browse through our range of national and regional
        level statistical subjects and find what you’re looking for.
      </p>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <h2 className="govuk-heading-m govuk-!-margin-bottom-0">
            <Link to="/prototypes/browse-releases">
              Find statistics and download data
            </Link>
          </h2>
          <p className="govuk-caption-m govuk-!-margin-top-2">
            Browse to find statistical summaries and download underlying data
            files to help you understand and analyse our range of education
            statistics.
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <h2 className="govuk-heading-m govuk-!-margin-bottom-0">
            <Link to="/prototypes/data-table-v1/national">
              Explore statistics and data online
            </Link>
          </h2>
          <p className="govuk-caption-m govuk-!-margin-top-2">
            Use our online chart and table builder tool to view, compare and
            contrast our range of education statistics and data.
          </p>
        </div>
      </div>
      <hr />
      <h3 className="govuk-heading-m govuk-!-margin-top-9">Related services</h3>
      <p className="govuk-body">
        Use these services to find information about individual schools and
        colleges
      </p>
      <div className="govuk-grid-row govuk-!-margin-bottom-9">
        <div className="govuk-grid-column-one-half">
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
            special needs schools and colleges
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            <a
              className="govuk-link"
              href="https://www.get-information-schools.service.gov.uk/"
            >
              Get information about schools
            </a>
          </h4>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            Search this register to find and download information about of
            schools and colleges in England including details educational
            organisations and governors
          </p>
        </div>
      </div>
    </PrototypePage>
  );
};

export default HomePage;
