import React from 'react';
// import CollapsibleSection from '../components/CollapsibleSection';
// import Details from '../components/Details';
import Link from '../components/Link';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';
import PrototypeTileWithChart from './components/PrototypeTileWithChart';

const HomePage = () => {
  return (
    <PrototypePage>
      <h1 className="govuk-heading-xl">Explore education statistics</h1>
      <p>
        Use this service to search for and find out about Department for
        Education (DfE) official statistics for England. The statistics can be
        viewed as reports, or you can customise and download as excel or .csv
        files. The data can also be accessed via an API.{' '}
        <Link to="#">Find out more</Link>
      </p>
      <PrototypeSearchForm />
      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        What statistics are you looking for?
      </h2>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-third dfe-dash-tiles__til">
          <Link
            to="/prototypes/theme"
            className="govuk-heading-m govuk-!-margin-bottom-0"
          >
            Schools
          </Link>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Data from Schools in England, including absence and exclusions,
            capacity and admissions, results, teacher numbers
          </p>
        </div>
        <div className="govuk-grid-column-one-third dfe-dash-tiles__til">
          <Link to="#" className="govuk-heading-m govuk-!-margin-bottom-0">
            16+ education
          </Link>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Data from further education, higher education and apprenticeships in
            England
          </p>
        </div>
        <div className="govuk-grid-column-one-third dfe-dash-tiles__til">
          <Link to="#" className="govuk-heading-m govuk-!-margin-bottom-0">
            Social care
          </Link>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Data from social care in England including number of children,
            vulnerable children numbers
          </p>
        </div>
      </div>
      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Highlights from latest publications
      </h2>
      <div className="dfe-dash-tiles dfe-dash-tiles--2-in-row govuk-!-margin-top-0 govuk-!-padding-top-0">
        <PrototypeTileWithChart
          heading="Pupil absence in schools in England"
          subheading="Sheffield overall absence"
          percent="4.7%"
        />
        <PrototypeTileWithChart
          heading="KS5 A-level results"
          subheading="Sheffield pupils with at least 2 A-levels"
          percent="72.7%"
        />
      </div>
      <Link to="#" className="govuk-button">
        Explore our statistics
      </Link>
    </PrototypePage>
  );
};

export default HomePage;
