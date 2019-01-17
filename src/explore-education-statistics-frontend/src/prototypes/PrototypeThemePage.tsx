import React from 'react';
// import CollapsibleSection from '../components/CollapsibleSection';
// import Details from '../components/Details';
import Link from '../components/Link';
import PrototypeDataTile from './components/PrototypeDataTile';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';
import PrototypeTileWithChart from './components/PrototypeTileWithChart';

const ThemePage = () => {
  return (
    <PrototypePage breadcrumbs={[{ text: 'Schools' }]}>
      <h1 className="govuk-heading-l">Schools</h1>
      <p className="govuk-body">
        Here you can find DfE statistics for schools. You can customise and
        download the statistics as excel files, and access them via an API.{' '}
        <a href="#">What is an API?</a>
      </p>
      <p>
        You can also see our statistics for <a href="#">16+ education</a> and{' '}
        <a href="#">social care</a>.
      </p>
      <PrototypeSearchForm />
      <h2 className="govuk-heading-m">What statistics are you looking for?</h2>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <a
            href="/prototypes/topic"
            className="govuk-heading-s govuk-!-margin-bottom-0"
          >
            Absence and exclusions
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Pupil absence, permanent and fixed period exclusions
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <a
            href="publication"
            className="govuk-heading-s govuk-!-margin-bottom-0"
          >
            School finance
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Schools, pupils and their characteristics, SEN and EHC plans, SEN in
            England
          </p>
        </div>
      </div>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <a href="#" className="govuk-heading-s govuk-!-margin-bottom-0">
            Capacity and exclusions
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            School capacity, admission appeals
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <a
            href="publication"
            className="govuk-heading-s govuk-!-margin-bottom-0"
          >
            School and pupil numbers
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Schools, pupils and their characteristics, SEN and EHC plans, SEN in
            England
          </p>
        </div>
      </div>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <a href="#" className="govuk-heading-s govuk-!-margin-bottom-0">
            Results
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Schools, pupils and their characteristics, SEN and EHC plans, SEN in
            England
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <a
            href="publication"
            className="govuk-heading-s govuk-!-margin-bottom-0"
          >
            Teacher numbers
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            School capacity, admission appeals
          </p>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-m">
        Latest publications in schools{' '}
        <Link to="#" className="govuk-body">
          (see all school publications)
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
          heading="KS5 A-level results"
          subheading="Sheffield pupils with at least 2 A-levels"
          percent="72.7%"
        />
      </div>
      <hr />
      <h2 className="govuk-heading-m">
        Key indicators for schools{' '}
        <Link to="#" className="govuk-body">
          (change)
        </Link>
      </h2>
      <p>
        These are some key indicators for schools. You can{' '}
        <Link to="#">change what you see here</Link> according to your
        requirements. <Link to="#">Find out more</Link>
      </p>
      <h3 className="govuk-heading-s">
        <Link to="#">Find an indicator &#x25BC;</Link>
      </h3>
      <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
        <PrototypeDataTile
          period="2017/18"
          heading="EYFSP good level of development"
          percent="70.3%"
          fromText="Early years foundation state profile results"
        />
        <PrototypeDataTile
          period="2016/17"
          heading="KS2 SAT expected standard"
          percent="3.4%"
          fromText="National curriculum assessments at Key Stage 2 in England"
        />
        <PrototypeDataTile
          period="2016/17"
          heading="KS4 GCSE average attainment 8 score"
          percent="1.3%"
          fromText="GCSE and equivalent results: 2017 to 2018 (provisional)"
        />
      </div>
      <a href="/prototypes/local-authority/data-table" className="govuk-button">
        Explore statistics
      </a>
    </PrototypePage>
  );
};

export default ThemePage;
