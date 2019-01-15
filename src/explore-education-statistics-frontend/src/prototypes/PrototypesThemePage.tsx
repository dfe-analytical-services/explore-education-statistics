import React from 'react';
// import CollapsibleSection from '../components/CollapsibleSection';
// import Details from '../components/Details';
// import PrototypeDataSample from './components/PrototypeDataSample';
import Link from '../components/Link';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';

const ThemePage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        { text: 'Schools', link: 'topic' },
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
      <h3>
        Key indicators for Sheffield{' '}
        <Link to="#" className="govuk-body">
          (Change key indicators)
        </Link>
      </h3>
      <p>
        These are some key indicators for Sheffield. You can change what you see
        here according your requirements. <Link to="#">Find out more</Link>
      </p>
      <h3 className="govuk-heading-s">
        <Link to="#">Find an indicator &#x25BC;</Link>
      </h3>
      <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
        <div className="dfe-dash-tiles__tile">
          <h3 className="govuk-heading-m">
            Permanent exclusion rate
            <span className="govuk-caption-m date-range govuk-tag">
              2016/17
            </span>
          </h3>
          <span className="govuk-heading-xl govuk-!-margin-bottom-0 govuk-caption-increase-negative">
            4.7%
          </span>
          <p className="govuk-body">
            <strong className="increase">
              +0.4
              <abbr aria-label="Percentage points" title="Percentage points">
                ppt
              </abbr>
            </strong>
            more than 2015/16
            <br />
            <a className="referenceLink" href="/local-authorities/sheffield">
              From: Early years foundation state profile results
            </a>
          </p>
          <details className="govuk-details">
            <summary className="govuk-details__summary">
              <span className="govuk-details__summary-text">
                What does this mean?
              </span>
            </summary>
            <div className="govuk-details__text">
              Permanent exclusion rate is the adipisicing elit. Dolorum hic
              nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </div>
          </details>
        </div>

        <div className="dfe-dash-tiles__tile">
          <h3 className="govuk-heading-m">
            Fixed period exlusion rate
            <span className="govuk-caption-m date-range govuk-tag">
              2016/17
            </span>
          </h3>
          <div>
            <span className="govuk-heading-xl govuk-!-margin-bottom-0 govuk-caption-increase-negative">
              3.4%
            </span>
            <p className="govuk-body">
              <strong className="level">
                0
                <abbr aria-label="Percentage points" title="Percentage points">
                  ppt
                </abbr>
              </strong>
              the same as 2015/16
              <br />
              <a className="referenceLink" href="/local-authorities/sheffield">
                From: National curriculum assessments at key stage 2 in England
              </a>
            </p>
          </div>
          <details className="govuk-details">
            <summary className="govuk-details__summary">
              <span className="govuk-details__summary-text">
                What does this mean?
              </span>
            </summary>
            <div className="govuk-details__text">
              Fixed period exlusion rateis the adipisicing elit. Dolorum hic
              nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </div>
          </details>
        </div>

        <div className="dfe-dash-tiles__tile">
          <h3 className="govuk-heading-m">
            One or more fixed period exclusion
            <span className="govuk-caption-m date-range govuk-tag">
              2016/17
            </span>
          </h3>
          <div>
            <span className="govuk-heading-xl govuk-!-margin-bottom-0 govuk-caption-increase-negative">
              1.3%
            </span>
            <p className="govuk-body">
              <strong className="decrease">
                -0.4
                <abbr aria-label="Percentage points" title="Percentage points">
                  ppt
                </abbr>
              </strong>
              more than 2015/16 <br />
              <a className="referenceLink" href="/local-authorities/sheffield">
                From: GCSE and equivalent results: 2017 to 2018 (provisional)
              </a>
            </p>
          </div>
          <details className="govuk-details">
            <summary className="govuk-details__summary">
              <span className="govuk-details__summary-text">
                What does this mean?
              </span>
            </summary>
            <div className="govuk-details__text">
              One or more fixed period exclusionis the adipisicing elit. Dolorum
              hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </div>
          </details>
        </div>
      </div>
      <a href="#" className="govuk-link">
        Add more or edit indicators
      </a>
    </PrototypePage>
  );
};

export default ThemePage;
