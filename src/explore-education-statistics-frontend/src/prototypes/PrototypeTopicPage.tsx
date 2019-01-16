import React from 'react';
// import CollapsibleSection from '../components/CollapsibleSection';
// import Details from '../components/Details';
// import PrototypeDataSample from './components/PrototypeDataSample';
import Link from '../components/Link';
import PrototypeDataTile from './components/PrototypeDataTile';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';
import PrototypeTileWithChart from './components/PrototypeTileWithChart';

const TopicPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        { text: 'Schools', link: '/prototypes/theme' },
        { text: 'Absence and exclusions', link: '#' },
      ]}
    >
      <h1 className="govuk-heading-l">
        Explore pupil absence statistics for schools in England
      </h1>
      <p className="govuk-body">
        Here you can find DfE stats for absence and exlusions, and access them
        as reports, customise and download as excel files, and access them via
        an API. <a href="#">Find out more</a>
      </p>
      <PrototypeSearchForm />
      <h2 className="govuk-heading-m">
        The following publications are available in absence and exclusions
      </h2>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <a
            href="publication"
            className="govuk-heading-s govuk-!-margin-bottom-0"
          >
            Pupil absence
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Overall absence, authorised absence, unauthorised absence,
            persisitent absence
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <a
            href="publication"
            className="govuk-heading-s govuk-!-margin-bottom-0"
          >
            Permananent and fixed period exclusions
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Permanent exclusions, and fixed period exclusions
          </p>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-m">
        Latest publications in absence and exclusions{' '}
        <Link to="#" className="govuk-body">
          (see all publications)
        </Link>
      </h2>
      <p>
        These are the latest official statistics with figures for Sheffield. You
        can access the report and commentary, and also get the data for use in
        Excel and other tools. You can now customise the data to your
        requirements, and get a variety of formats.
      </p>
      <div className="dfe-dash-tiles dfe-dash-tiles--2-in-row govuk-!-margin-top-0 govuk-!-padding-top-0">
        <PrototypeTileWithChart
          heading="Pupil absence in schools in England"
          subheading="Sheffield overall absence"
          percent="4.7%"
        />
        <PrototypeTileWithChart
          heading="Permanent and fixed period exclusions"
          subheading="Overall rate of fixed period exclusions"
          percent="0.10%"
        />
      </div>
      <hr />
      <h2 className="govuk-heading-m">
        Key indicators for Sheffield{' '}
        <Link to="#" className="govuk-body">
          (change key indicators)
        </Link>
      </h2>
      <p>
        These are some key indicators for Sheffield. You can change what you see
        here according your requirements. <Link to="#">Find out more</Link>
      </p>

      <h3 className="govuk-heading-s">
        <Link to="#">Find an indicator &#x25BC;</Link>
      </h3>
      <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
        <PrototypeDataTile
          period="2016/17"
          heading="Permanent exclusion rate"
          percent="4.7%"
          fromText="Early years foundation state profile results"
        />
        <PrototypeDataTile
          period="2016/17"
          heading="Fixed period exclusion rate"
          percent="3.4%"
          fromText="National curriculum assessments at Key Stage 2 in England"
        />
        <PrototypeDataTile
          period="2016/17"
          heading="One or more fixed period exclusion"
          percent="1.3%"
          fromText="GCSE and equivalent results: 2017 to 2018 (provisional)"
        />
      </div>
      <a href="#" className="govuk-button">
        Explore statistics
      </a>
    </PrototypePage>
  );
};

export default TopicPage;
