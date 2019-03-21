import React from 'react';
import Link from '../components/Link';
import PrototypePage from './components/PrototypePage';

const HomePage = () => {
  return (
    <PrototypePage>
      <h1 className="govuk-heading-xl">Methodology and guidance</h1>
      <p className="govuk-body-l">
        Information on the methods we use to produce our statistics. This
        includes classifications, harmonisation, best practice, geography and
        user guidance for a wide range of data.
      </p>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <h2 className="govuk-heading-m govuk-!-margin-bottom-1">
            <Link to="#">General methodology</Link>
          </h2>
          <p className="govuk-body">
            How we collect and process data, what we do with it and related
            policies.
          </p>

          <h2 className="govuk-heading-m govuk-!-margin-bottom-1">
            <Link to="#">Specific methodology and guidance</Link>
          </h2>
          <p className="govuk-body">
            Find methodology and guidance for specific DfE publications.
          </p>

          <h2 className="govuk-heading-m govuk-!-margin-bottom-1">
            <Link to="#">Glossary</Link>
          </h2>
          <p className="govuk-body">
            A comprehensive list of decriptions for terms used in our
            publications.
          </p>
        </div>
      </div>
      <hr />
      <h3 className="govuk-heading-m govuk-!-margin-top-9">Related services</h3>
      <p className="govuk-body">
        Use these services to find and compare and contrast performance and
        other information about schools and colleges near you:
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
